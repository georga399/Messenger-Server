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
    private readonly IMapper _mapper;
    private readonly ILogger<MessengerHub> _logger;
    private readonly IUnitOfWork _unitOfWork;
    public MessengerHub(IMapper mapper, 
        ILogger<MessengerHub> logger, IUnitOfWork unitOfWork)
    {
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
        await Clients.Clients(connectionsOfChat!.Select(c=>c.ConnectionID))
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
        await Clients.Clients(connectionsOfChat!.Select(c => c.ConnectionID))
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
        await Clients.Clients(connectionsOfChat!.Select(c => c.ConnectionID))
            .SendAsync("OnJoinChat", chatViewModel);
        
    }
    public async Task DeleteChat(int chatId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = _unitOfWork.UserRepository.GetById(userId!);
        var chatsOfUser = await _unitOfWork.ChatRepository.GetAllChatsOfUserAsync(userId!);
        var chat = chatsOfUser!.FirstOrDefault(c => c.Id == chatId);
        if(chat == null)
        {
            await Clients.Caller.SendAsync("OnError", "Chat not found");
            return;
        }
        var res = await _unitOfWork.ChatRepository.DeleteChatIfAdmin(chatId, userId!);
        if(!res)
        {
            await Clients.Caller.SendAsync("OnError", "Permission denied");
            return;
        }
        await _unitOfWork.SaveChangesAsync();
        var connectionsOfChat = await _unitOfWork.ConnectionRepository.GetAllConnectionsOfChat(chat.Id);
        await Clients.Clients(connectionsOfChat!.Select(c => c.ConnectionID))
            .SendAsync("OnDeleteChat", chatId);
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
        var user = _unitOfWork.UserRepository.GetById(userId);
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
        await Clients.Clients(connectionsOfUser!.Select(c => c.ConnectionID))
            .SendAsync("OnJoinChat", chatViewModel);

        var  connectionsOfChat = await _unitOfWork.ConnectionRepository.GetAllConnectionsOfChat(chatId);
        await Clients.Clients(connectionsOfChat!.Select(c => c.ConnectionID))
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
        var  connectionsOfChat = await _unitOfWork.ConnectionRepository.GetAllConnectionsOfChat(chatid);

        if(await _unitOfWork.ChatRepository.DeleteChatIfAdmin(chatid, userId!))
        {
            await _unitOfWork.SaveChangesAsync();
            await Clients.Clients(connectionsOfChat!.Select(c => c.ConnectionID))
            .SendAsync("OnDeleteChat", chatid);
        }
        else
        {
            await _unitOfWork.ChatRepository.LeaveChat(chatid, userId!);
            await _unitOfWork.SaveChangesAsync();
            await Clients.Clients(connectionsOfChat!.Select(c => c.ConnectionID))
            .SendAsync("OnLeaveChat", userId, chatid);
        }
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
        await Clients.Clients(connectionsOfUser!.Select(c => c.ConnectionID))
            .SendAsync("OnSetLastReadMessage", chatId, messageId);
    }
    public override async Task<Task> OnConnectedAsync()
    {
        await _unitOfWork.ConnectionRepository.Add(Context.GetHttpContext()!.User
            .FindFirstValue(ClaimTypes.NameIdentifier)!, Context.ConnectionId);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation($"User {Context.ConnectionId} was connected");
        return base.OnConnectedAsync();
    }
    public override async Task<Task> OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"User {Context.ConnectionId} was disconnected");
        await _unitOfWork.ConnectionRepository.Remove(Context.ConnectionId);
        await _unitOfWork.SaveChangesAsync();
        return base.OnDisconnectedAsync(exception);
    }
}