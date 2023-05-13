using Messenger.Models;
using Messenger.ViewModels;
namespace Messenger.Repositories;
public interface IChatRepository
{
    // void AddGroup(Chat chat);
    Chat? AddChat(ChatViewModel chatViewModel); //Return true with success
    void Remove(Chat chat);
    Task<bool> Remove(int chatId);
    Task JoinChat(int ChatId, string userId);
    Task<bool> LeaveChat(int ChatId, string userId);
    Task<List<ChatUser>?> GetAllMembers(int ChatId);
    Task<Chat?> GetChatInfoAsync(int ChatId);    
    Task<List<Chat>?> GetAllChatsOfUserAsync(string userId);
    Task<bool> SetAdmin(int chatId, string userId); //Return true with success
}