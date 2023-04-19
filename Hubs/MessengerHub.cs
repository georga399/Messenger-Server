using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization; 
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;   
using Messenger.Models;
using Messenger.ViewModels;
using Messenger.Data;  
namespace Messenger.Hubs;

[Authorize]
public class MessengerHub: Hub
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILogger<MessengerHub> _logger;

    public MessengerHub(ApplicationDbContext dbContext, IMapper mapper, ILogger<MessengerHub> logger)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task SendMessage(int chatId, MessageViewModel messageViewModel)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId == null) 
        {
            await this.Clients.Caller.SendAsync("OnError", "Current user not found");
            return;
        }
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if(user == null)
        {
            await this.Clients.Caller.SendAsync("OnError", "Current user not found");
            return;
        } 
        Message message = _mapper.Map<MessageViewModel, Message>(messageViewModel);
        var chat = await _dbContext.Chats.FirstOrDefaultAsync(ch => ch.Id == message.ChatId);
        if(chat == null) 
        {
            await this.Clients.Caller.SendAsync("OnError", "Chat not found");
            return;
        }
        message.FromUser = user;
        message.Chat = chat;
        message.FromUserIntId = user.IntId;
        message.Timestamp = DateTime.UtcNow;
        //Saving to DB
        await _dbContext.Messages.AddAsync(message);
        await _dbContext.SaveChangesAsync();
        messageViewModel = _mapper.Map<Message, MessageViewModel>(message);
        //Sending to clients
        await _dbContext.Entry(chat).Collection(c => c.Users).Query().Where(u => u.Connections.Count > 0).LoadAsync();
        foreach(var usr in chat.Users)
            foreach(var cnctn in usr.Connections)
                await Clients.Client(cnctn.ConnectionID).SendAsync("OnSendMessage", messageViewModel);
    }
    public async Task DeleteMessage(int chatId, int messageId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId == null) 
        {
            await this.Clients.Caller.SendAsync("OnSendMessage", "Current user not found");
            return;
        }
        var user = await _dbContext.Users.Include(u => u.Chats).ThenInclude(ch => ch.Messages).FirstOrDefaultAsync(u => u.Id == userId);
        if(user == null)
        {
            await this.Clients.Caller.SendAsync("OnError", "Current user not found");
            return;
        } 
        if(user == null) 
        {
            await Clients.Caller.SendAsync("OnError", "Current user not found");
            return;
        }
        var chat = user.Chats.FirstOrDefault(ch => ch.Id == chatId);
        if(chat == null)
        {
            await Clients.Caller.SendAsync("OnError", "Chat not found");
            return;
        } 
        var msg = chat.Messages.FirstOrDefault(m => m.Id == messageId);
        if(msg == null)
        {
            await Clients.Caller.SendAsync("OnError", "Message not found");
            return;
        }
        chat.Messages.Remove(msg);
        await _dbContext.SaveChangesAsync();
        //Sending to clients
        await _dbContext.Entry(chat).Collection(c => c.Users).Query().Where(u => u.Connections.Count > 0).LoadAsync();
        foreach(var usr in chat.Users)
            foreach(var cnctn in usr.Connections)
                await Clients.Client(cnctn.ConnectionID).SendAsync("OnDeleteMessage", chatId, messageId);
    }
    public async Task CreateGroupChat(ChatViewModel chatViewModel)
    {
        if(!chatViewModel.IsGroup)
        {
            await Clients.Caller.SendAsync("OnError", "Chat should be a group");
            return;
        }
        Chat chat = _mapper.Map<ChatViewModel, Chat>(chatViewModel);
        await _dbContext.Chats.AddAsync(chat);
        foreach(var usrId in chatViewModel.UsersId)
        {
            var usr = _dbContext.Users.FirstOrDefault(u => u.IntId == usrId);
            if(usr == null)
            {
                _dbContext.Remove(chat);
                await Clients.Caller.SendAsync("OnError", $"User with id={usrId} not found");
                return;
            }
            ChatUser cu = new ChatUser{Chat = chat, User = usr};
            usr.ChatUsers.Add(cu);
            usr.Chats.Add(chat);
            chat.ChatUsers.Add(cu);
            chat.Users.Add(usr);
        }
        await _dbContext.SaveChangesAsync();
        //Sending to clients
        await _dbContext.Entry(chat).Collection(c => c.Users).Query().Where(u => u.Connections.Count > 0).LoadAsync();
        foreach(var usr in chat.Users)
            foreach(var cnctn in usr.Connections)
                await Clients.Client(cnctn.ConnectionID).SendAsync("OnJoinChat", chat.Id); //REWRITE TO RETURN CHATVIEWMODEL
    }
    public async Task JoinChat(int chatId, string userId)
    {
        var inviterId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if(inviterId == null) 
        {
            await Clients.Caller.SendAsync("OnError", "Inviter not found");
            return;
        }
        var inviter = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == inviterId);
        if(inviter == null)
        {
            await Clients.Caller.SendAsync("OnError", "Inviter not found");
            return;
        }
        var user = await _dbContext.Users.FirstOrDefaultAsync(u=> u.Id == userId);
        if(user == null)
        {
            await Clients.Caller.SendAsync("OnError", "User not found");
            return;
        }
        var chat = user.Chats.FirstOrDefault(ch => ch.Id == chatId);
        if(chat == null)
        {
            await Clients.Caller.SendAsync("OnError", "Chat not found");
            return;
        }
        ChatUser chatUser = new ChatUser{Chat = chat, User = user};
        user.ChatUsers.Add(chatUser);
        user.Chats.Add(chat);
        chat.ChatUsers.Add(chatUser);
        chat.Users.Add(user);
        _dbContext.SaveChanges();
        //TODO: Notify others about joining user in this chat
        //Sending to clients
        await _dbContext.Entry(chat).Collection(c => c.Users).Query().Where(u => u.Id == inviterId).LoadAsync();
        foreach(var usr in chat.Users)
            foreach(var cnctn in usr.Connections)
                await Clients.Client(cnctn.ConnectionID).SendAsync("OnJoinChat", chatId);
    }
    public async Task LeaveChat(int chatid)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId == null) 
        {
            await Clients.Caller.SendAsync("OnError", "Inviter not found");
            return;
        }
        var user = await _dbContext.Users.Include(u => u.Chats).FirstOrDefaultAsync(u => u.Id == userId);
        if(user == null)
        {
            await Clients.Caller.SendAsync("OnError", "Inviter not found");
            return;
        }
        var chat = user.Chats.FirstOrDefault(ch => chatid == ch.Id);
        if(chat == null)
        {
            await Clients.Caller.SendAsync("OnError", "Chat is not found");
            return;
        }
        user.Chats.Remove(chat);
        await _dbContext.SaveChangesAsync();
        await Clients.Caller.SendAsync("OnLeaveChat", $"You left chat chatid={chatid}");
    }
    public override async Task<Task> OnConnectedAsync()
    {
        var name = Context.User?.Identity?.Name;
        var user = _dbContext.Users
            .Include(u => u.Connections)
            .SingleOrDefault(u => u.UserName == name);
        if (user == null)
        {
            await Clients.Caller.SendAsync("OnError", "User not found");
            return base.OnConnectedAsync();
        }
        user.Connections.Add(new Connection
        {
            ConnectionID = Context.ConnectionId,
        });
        await _dbContext.SaveChangesAsync();
        return base.OnConnectedAsync();
    }
    public override async Task<Task> OnDisconnectedAsync(Exception? exception)
    {
        var connections = await _dbContext.Connections.ToListAsync();
        var connection = connections.FirstOrDefault(c => c.ConnectionID == Context.ConnectionId);
        if(connection != null) connections.Remove(connection);
        await _dbContext.SaveChangesAsync();
        return base.OnDisconnectedAsync(exception);
    }
}