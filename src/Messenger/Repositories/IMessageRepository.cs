using Messenger.Models;
namespace Messenger.Repositories;
public interface IMessageRepository
{
    Task Add(Message message);
    Task<Message?> GetMessageById(int messageId);
    Task<Message?> GetMessageByIdInChat(int messageId, int chatId);
    Task Update(Message message);
    Task Remove(Message message);
    Task Remove(int messageId);
    Task<List<Message>?> GetMessagesRangeAsync(int chatId, int messageId, int range);
    Task<bool> SetLastReadMessageAsync(string userId, int chatId, int messageId);
    Task<int?> GetNewestMessageIdAsync(int chatId);
    Task<int?> GetLastReadMessageIdAsync(string userId, int chatId);
}