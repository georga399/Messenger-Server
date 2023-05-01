using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization; 
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;   
using Messenger.Models;
using Messenger.ViewModels;
using Messenger.Data;
using Messenger.Repositories;  
namespace Messenger.Hubs;

[Authorize]
public class MessengerHub: Hub
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILogger<MessengerHub> _logger;
    private readonly IUnitOfWork _unitOfWork;
    public MessengerHub(ApplicationDbContext dbContext, IMapper mapper, 
        ILogger<MessengerHub> logger, IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
    public async Task SendMessage(int chatId, MessageViewModel messageViewModel)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = _unitOfWork.UserRepository.GetById(userId!);
        Message message = _mapper.Map<MessageViewModel, Message>(messageViewModel);
        var chats = await _unitOfWork.ChatRepository.GetAllChatsOfUserAsync(userId!);
        var chat = chats!.FirstOrDefault(c => c.Id == messageViewModel.ChatId);
        if(chat == null) 
        {
            await this.Clients.Caller.SendAsync("OnError", "Chat not found");
            return;
        }
        message.FromUser = user;
        message.Chat = chat;
        message.Timestamp = DateTime.UtcNow;
        //Saving to DB
        await _unitOfWork.MessageRepository.Add(message);
        await _unitOfWork.SaveChangesAsync();
        //Sending to clients
        messageViewModel = _mapper.Map<Message, MessageViewModel>(message);
        var connectionsOfChat = await _unitOfWork.ConnectionRepository.GetAllConnectionsOfChat(chatId);
        await Clients.Clients((from t in connectionsOfChat 
            where true select t.ConnectionID)
            .ToList())
            .SendAsync("OnSendMessage", messageViewModel);
    }
    public async Task DeleteMessage(int chatId, int messageId) 
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = _unitOfWork.UserRepository.GetById(userId!);
        var chatsOfUser = await _unitOfWork.ChatRepository.GetAllChatsOfUserAsync(userId!);
        var chat = chatsOfUser!.FirstOrDefault(c => c.Id == chatId);
        if(chat == null)
        {
            await this.Clients.Caller.SendAsync("OnError", "Chat not found");
            return;
        }
        var msg = await _unitOfWork.MessageRepository.GetMessageByIdInChat(messageId, chatId);
        if(msg == null)
        {
            await Clients.Caller.SendAsync("OnError", "Message not found");
            return;
        }
        if(!chat.IsGroup && (chat.Admin != user || msg.FromUser != user))
        {
            await Clients.Caller.SendAsync("OnError", "Permission denied");
        }
        //Deleting
        await _unitOfWork.MessageRepository.Remove(msg);
        await _unitOfWork.SaveChangesAsync();
        //Sending to clients
        var connectionsOfChat = await _unitOfWork.ConnectionRepository.GetAllConnectionsOfChat(chatId);        
        await Clients.Clients((from t in connectionsOfChat 
            where true select t.ConnectionID)
            .ToList())
            .SendAsync("OnDeleteMessage", chatId, messageId);
    }
    public async Task CreateChat(ChatViewModel chatViewModel)
    {
        
        var chat = _unitOfWork.ChatRepository.AddChat(chatViewModel);
        if(chat == null)
        {
            await Clients.Caller.SendAsync("OnError", "Chat wasn't created!");
            return;
        }
        await _unitOfWork.SaveChangesAsync();
        chatViewModel.Id = chat.Id;
        //Sending to clients
        var connectionsOfChat = await _unitOfWork.ConnectionRepository.GetAllConnectionsOfChat(chat.Id);        
        await Clients.Clients((from t in connectionsOfChat 
            where true select t.ConnectionID)
            .ToList())
            .SendAsync("OnJoinChat", chatViewModel);
        
    }
    public async Task JoinChat(int chatId, string userId)
    {
        var inviterId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var chat = await _unitOfWork.ChatRepository.GetChatInfoAsync(chatId);
        //Validation
        if(chat == null)
        {
            await Clients.Caller.SendAsync("OnError", "Chat not found");
            return;
        }
        var user = await _dbContext.Users.FirstOrDefaultAsync(u=> u.Id == userId);
        if(user == null)
        {
            await Clients.Caller.SendAsync("OnError", "User not found");
            return;
        }
        if(chat.IsGroup && (chat.AdminId != inviterId && inviterId != userId) || 
            !chat.IsGroup && chat.AdminId !=inviterId)
        {
            await Clients.Caller.SendAsync("OnError", "Permission denied");
            return;   
        }
        //Saving to db
        await _unitOfWork.ChatRepository.JoinChat(chatId, userId);
        await _unitOfWork.SaveChangesAsync();
        //Sending to clients
        var connectionsOfUser =await _unitOfWork.ConnectionRepository.GetConnectionsOfUser(userId);
        var chatViewModel = _mapper.Map<Chat, ChatViewModel>(chat);
        await Clients.Clients((from t in connectionsOfUser 
            where true select t.ConnectionID)
            .ToList())
            .SendAsync("OnJoinChat", chatViewModel);

        var  connectionsOfChat = await _unitOfWork.ConnectionRepository.GetAllConnectionsOfChat(chatId);
        await Clients.Clients((from t in connectionsOfChat 
            where true select t.ConnectionID)
            .ToList())
            .SendAsync("OnAddedUserToChat", userId);
    }
    public async Task LeaveChat(int chatid)
    {   
        //Validation
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var chatsOfUser = await _unitOfWork.ChatRepository.GetAllChatsOfUserAsync(userId!);
        var chat = chatsOfUser?.FirstOrDefault(c => c.Id == chatid);
        if(chat == null)
        {
            await Clients.Caller.SendAsync("OnError", "Chat is not found");
            return;
        }
        //Saving to the db
        await _unitOfWork.ChatRepository.LeaveChat(chatid, userId!);
        await _unitOfWork.SaveChangesAsync();
        //Send to the client
        var  connectionsOfChat = await _unitOfWork.ConnectionRepository.GetAllConnectionsOfChat(chatid);
        await Clients.Clients((from t in connectionsOfChat 
            where true select t.ConnectionID)
            .ToList())
            .SendAsync("OnLeaveChat", $"UserId={userId} left ChatId={chatid}");
    }
    public async Task SetLastReadMessage(int chatId, int messageId) 
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var res = await _unitOfWork.MessageRepository.SetLastReadMessageAsync(userId!, chatId, messageId);
        if(!res)
        {
            await Clients.Caller.SendAsync("OnError", "Something went wrong");
            return;
        }
        await _unitOfWork.SaveChangesAsync();
        var connectionsOfUser = await _unitOfWork.ConnectionRepository.GetConnectionsOfUser(userId!);
        await Clients.Clients((from t in connectionsOfUser 
            where true select t.ConnectionID)
            .ToList())
            .SendAsync("OnSetLastReadMessage", chatId, messageId);
    }
    public override async Task<Task> OnConnectedAsync()
    {
        await _unitOfWork.ConnectionRepository.Add(Context.GetHttpContext()!.User
            .FindFirstValue(ClaimTypes.NameIdentifier)!, Context.ConnectionId);
        await _unitOfWork.SaveChangesAsync();
        return base.OnConnectedAsync();
    }
    public override async Task<Task> OnDisconnectedAsync(Exception? exception)
    {
        await _unitOfWork.ConnectionRepository.Remove(Context.ConnectionId);
        await _unitOfWork.SaveChangesAsync();
        return base.OnDisconnectedAsync(exception);
    }
}