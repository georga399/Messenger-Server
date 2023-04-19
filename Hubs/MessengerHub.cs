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
    //TODO: optimize sending notifies / saving connectionId to DB
    private readonly static Dictionary<string, HashSet<string>> _ConnectionsMap = new Dictionary<string, HashSet<string>>();
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    public MessengerHub(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    public async Task SendMessage(int chatId, MessageViewModel messageViewModel)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId == null) 
        {
            await this.Clients.Caller.SendAsync("OnSendMessage", "Current user not found");
            return;
        }
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if(user == null)
        {
            await this.Clients.Caller.SendAsync("OnError", "Current user not found");
            return;
        } 
        Message message = _mapper.Map<MessageViewModel, Message>(messageViewModel);
        var chat = await _dbContext.Chats.Include(ch => ch.Users).FirstOrDefaultAsync(ch => ch.Id == message.ChatId);
        if(chat == null) 
        {
            await this.Clients.Caller.SendAsync("OnSendMessage", "Chat not found");
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
        foreach(var usr in chat.Users) //O(n*n)
        {
            HashSet<string>? userConnections;
            if(_ConnectionsMap.TryGetValue(userId, out userConnections))
            {
                await Clients.Clients(userConnections).SendAsync("OnSendMessage", messageViewModel);
            }
        }
    }
    public async Task DeleteMessage(int chatId, int messageId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId == null) 
        {
            await this.Clients.Caller.SendAsync("OnSendMessage", "Current user not found");
            return;
        }
        //TODO: MAKE NORMAL LOADING
        var user = await _dbContext.Users.Include(u => u.Chats).ThenInclude(ch => ch.Messages).Include(u => u.Chats).ThenInclude(ch => ch.Users).FirstOrDefaultAsync(u => u.Id == userId);
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
        foreach(var usr in chat.Users) //O(n*n)
        {
            HashSet<string>? userConnections;
            if(_ConnectionsMap.TryGetValue(userId, out userConnections))
            {
                await Clients.Clients(userConnections).SendAsync("OnSendMessage", chatId, messageId);
            }
        }
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
        foreach(var usr in chat.Users) //O(n*n)
        {
            HashSet<string>? userConnections;
            if(_ConnectionsMap.TryGetValue(usr.Id, out userConnections))
            {
                await Clients.Clients(userConnections).SendAsync("OnJoinChat", chat.Id);
            }
        }
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
        HashSet<string>? userConnections;
        if(_ConnectionsMap.TryGetValue(user.Id, out userConnections))
        {
            await Clients.Clients(userConnections).SendAsync("OnJoinChat", chat.Id);
        }
        //TODO: Notify others about joining user in this chat
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
        try
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if(userId == null || user == null) return base.OnConnectedAsync();
            HashSet<string>? connections;
            if (!_ConnectionsMap.TryGetValue(userId, out connections))
            {
                connections = new HashSet<string>();
                _ConnectionsMap.Add(userId, connections);
            }
            connections.Add(Context.ConnectionId);
        }
        catch(Exception ex)
        {
            await Clients.Caller.SendAsync("OnError", "OnConnected: " + ex.Message);
        }
        return base.OnConnectedAsync();
    }
    public override async Task<Task> OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if(userId == null || user == null) return base.OnConnectedAsync();
            HashSet<string>? connections;
            if (!_ConnectionsMap.TryGetValue(userId, out connections))
            {
                return base.OnDisconnectedAsync(exception);
            }
            connections.Remove(Context.ConnectionId);
            if(connections.Count == 0) _ConnectionsMap.Remove(userId);
        }
        catch(Exception ex)
        {
            await Clients.Caller.SendAsync("OnError", "OnDisconnected: " + ex.Message);
        }
        return base.OnDisconnectedAsync(exception);
    }
}