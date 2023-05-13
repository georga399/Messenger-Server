using Messenger.Models;
using Messenger.ViewModels;
using Messenger.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Messenger.Repositories;
public class ChatRepository: IChatRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILogger<ChatRepository> _logger;
    public ChatRepository(ApplicationDbContext dbContext, IMapper mapper, ILogger<ChatRepository> logger)
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _logger = logger;
    }
    public Chat? AddChat(ChatViewModel chatViewModel) 
    {
        Chat chat = _mapper.Map<ChatViewModel, Chat>(chatViewModel);
        bool addedAdmin = false;
        _dbContext.Chats.Add(chat);
        foreach(var usrId in chatViewModel.UsersId)
        {
            _logger.LogInformation($"Adding user = {usrId} to the chat {chat.Title}");
            var usr = _dbContext.Users.FirstOrDefault(u => u.Id == usrId);
            if(usr == null) //TODO: DELETING CHAT
            {
                _logger.LogInformation($"User = {usrId} not found in chat {chat.Title}");                
                continue;
            }
            ChatUser cu = new ChatUser{Chat = chat, User = usr, UserId = usr.Id, ChatId = chat.Id};
            // usr.ChatUsers.Add(cu);
            chat.ChatUsers.Add(cu);
            if(usrId == chatViewModel.AdminId)
            {
                usr.AdministrateChats.Add(chat);
                chat.Admin = usr;
                addedAdmin = true;
            }
        }
        if(!addedAdmin)
        {
            _dbContext.Chats.Remove(chat);
            return null;
        }
        return chat;
    }
    public void Remove(Chat chat)
    {
        _dbContext.Chats.Remove(chat);
    }
    public async Task<bool> Remove(int chatId)
    {

        var chat = await _dbContext.Chats.FirstOrDefaultAsync(ch => ch.Id == chatId);
        if(chat == null)
        {
            //TODO: Notify about incorrect chatId
            return false;
        }
        else
        {
            _dbContext.Chats.Remove(chat);
            return true;
        }
    }
    public async Task<bool> JoinChat(int ChatId, string userId)
    {        
        var user = await _dbContext.Users
        .Include(u => u.ChatUsers)
        .FirstOrDefaultAsync(u=> u.Id == userId);
        if(user == null)
        {
            return false;
        }
        var existedChatUser = user.ChatUsers.FirstOrDefault(cu => cu.ChatId == ChatId);
        if(existedChatUser != null)
        {
            return false;
        }
        var chat = await _dbContext.Chats.FirstOrDefaultAsync(ch => ch.Id == ChatId);
        if(chat == null)
        {
            return false;
        }
        ChatUser cu = new ChatUser()
        {
            Chat = chat,
            User = user,
            // ChatId = chat.Id,
            // UserId = user.Id
        };
        chat.ChatUsers.Add(cu);
        user.ChatUsers.Add(cu);
        return true;
    }
    public async Task<bool> LeaveChat(int ChatId, string userId) //TODO: Explicit downloading
    {
        var user = await _dbContext.Users
        .Include(u => u.ChatUsers)
        .ThenInclude(cu => cu.Chat)
        .ThenInclude(ch => ch.Admin)
        .FirstOrDefaultAsync(u=> u.Id == userId);
        if(user == null)
        {
            return false;
        }
        var chatUser = user.ChatUsers.FirstOrDefault(cu => cu.ChatId == ChatId);
        if(chatUser == null)
        {
            return false;
        }
        user.ChatUsers.Remove(chatUser);
        chatUser.Chat.ChatUsers.Remove(chatUser);
        if(chatUser.Chat.Admin!.Id == userId)
        {
            _dbContext.Chats.Remove(chatUser.Chat);
        }
        return true;        
    }
    public async Task<List<ChatUser>?> GetAllMembers(int ChatId)
    {
        var chat = await _dbContext.Chats
        .Include(ch => ch.ChatUsers)
        // ThenInclude(cu => cu.User)
        .FirstOrDefaultAsync(c => c.Id == ChatId);
        if(chat == null)
        {
            return null;
        }
        return chat.ChatUsers;
    }
    public async Task<Chat?> GetChatInfoAsync(int ChatId)
    {
        var chat = await _dbContext.Chats
        .Include(c => c.ChatUsers)
        .FirstOrDefaultAsync(c => c.Id == ChatId);
        return chat;
    }
    public async Task<List<Chat>?> GetAllChatsOfUserAsync(string userId)
    {
        var user = await _dbContext.Users
        .Include(u => u.ChatUsers)
        .ThenInclude(cu => cu.Chat)
        .FirstOrDefaultAsync(u => u.Id == userId);
        if(user == null)
        {
            return null;
        }
        return user.ChatUsers.Select(cu => cu.Chat).ToList();
    }
    public async Task<bool> SetAdmin(int chatId, string userId) //TODO: Explicit loading
    {
        var chat = await _dbContext.Chats
        .Include(c => c.ChatUsers)
        .ThenInclude(cu => cu.User)
        .Include(c => c.Admin)
        .FirstOrDefaultAsync(c => c.Id == chatId);
        if(chat == null)
        {
            return false;
        }
        var chatUser = chat.ChatUsers.FirstOrDefault(cu => cu.UserId == userId);
        if(chatUser == null)
        {
            return false;
        }
        if(chat.Admin!.Id == userId)
        {
            return false;
        }
        chat.Admin?.AdministrateChats.Remove(chat);
        chat.Admin = chatUser.User;
        return true;
    }
    public async Task<bool> DeleteChatIfAdmin(int chatId, string userId)
    {
        var user = await _dbContext.Users
        .Include(u => u.ChatUsers)
        .ThenInclude(cu => cu.Chat)
        .ThenInclude(c => c.Admin)
        .FirstOrDefaultAsync(u => u.Id == userId);
        if(user == null) return false;
        var chatUser = user.ChatUsers.FirstOrDefault(cu => cu.ChatId == chatId);
        if(chatUser == null) return false;
        if(chatUser.Chat.AdminId != userId) return false;
        _dbContext.Chats.Remove(chatUser.Chat);
        return true;
    }
}