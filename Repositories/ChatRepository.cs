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
    public ChatRepository(ApplicationDbContext dbContext, IMapper mapper)
    {
        _mapper = mapper;
        _dbContext = dbContext;
    }
    public Chat? AddChat(ChatViewModel chatViewModel) 
    {
        Chat chat = _mapper.Map<ChatViewModel, Chat>(chatViewModel);
        bool addedAdmin = false;
        foreach(var usrId in chatViewModel.UsersId)
        {
            var usr = _dbContext.Users.FirstOrDefault(u => u.Id == usrId);
            if(usr == null)
            {
                _dbContext.Remove(chat);
                return null;
            }
            ChatUser cu = new ChatUser{Chat = chat, User = usr};
            usr.ChatUsers.Add(cu);
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
            _dbContext.Remove(chat);
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
    public async Task JoinChat(int ChatId, string userId)
    {        
        var user = await _dbContext.Users
        .FirstOrDefaultAsync(u=> u.Id == userId);
        if(user == null)
        {
            //TODO: Notify about user not found
            return;
        }
        var chat = await _dbContext.Chats.FirstOrDefaultAsync(ch => ch.Id == ChatId);
        if(chat == null)
        {
            //TODO: Notify about chat not found
            return;
        }
        ChatUser cu = new ChatUser()
        {
            Chat = chat,
            User = user
        };
        chat.ChatUsers.Add(cu);
        user.ChatUsers.Add(cu);
    }
    public async Task<bool> LeaveChat(int ChatId, string userId) //TODO: Explicit downloading
    {
        var user = await _dbContext.Users
        .Include(u => u.ChatUsers)
        .ThenInclude(cu => cu.Chat)
        .FirstOrDefaultAsync(u=> u.Id == userId);
        if(user == null)
        {
            //TODO: Notify about user not found
            return false;
        }
        var chatUser = user.ChatUsers.FirstOrDefault(cu => cu.ChatId == ChatId);
        if(chatUser == null)
        {
            //TODO: Notify about chat Not founded
            return false;
        }
        user.ChatUsers.Remove(chatUser);
        chatUser.Chat.ChatUsers.Remove(chatUser);
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
            //Notify about chat == null
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
            //TODO: Notify about user not found
            return null;
        }
        return (from t in user.ChatUsers where true select t.Chat).ToList();
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
            //TODO: Notify about it
            return false;
        }
        var chatUser = chat.ChatUsers.FirstOrDefault(cu => cu.UserId == userId);
        if(chatUser == null)
        {
            //TODO: Notify about it
            return false;
        }
        chat.Admin?.AdministrateChats.Remove(chat);
        chat.Admin = chatUser.User;
        return true;
    }
}