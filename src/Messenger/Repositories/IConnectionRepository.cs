using Messenger.Models;
namespace Messenger.Repositories;
public interface IConnectionRepository
{
    Task Add(string userId, string connectionId);
    Task Remove(string connectionId);
    Task<List<Connection>?> GetConnectionsOfUser(string userId);
    Task<List<Connection>?> GetAllConnectionsOfChat(int chatId);

}