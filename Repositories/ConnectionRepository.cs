using Messenger.Data;
using Messenger.Models;
using Microsoft.EntityFrameworkCore;

namespace Messenger.Repositories;
public class ConnectionRepository: IConnectionRepository
{
    private readonly ApplicationDbContext _dbContext;
    public ConnectionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Add(string userId, string connectionId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if(user == null)
        {
            //TODO: Notify about it
            return;
        }
        user.Connections.Add(new Connection{ConnectionID = connectionId});
    }
    public async Task Remove(string connectionId)
    {
        var connections = await _dbContext.Connections.ToListAsync();
        var connection = connections.FirstOrDefault(c => c.ConnectionID == connectionId);
        if(connection != null) connections.Remove(connection);
    }
    public async Task<List<Connection>?> GetConnectionsOfUser(string userId)
    {
        var user = await _dbContext.Users
        .Include(u => u.Connections)
        .FirstOrDefaultAsync(u => u.Id == userId);
        return user?.Connections;
    }
    public async Task<List<Connection>?> GetAllConnectionsOfChat(int chatId)
    {
        var chat = await _dbContext.Chats
        .FirstOrDefaultAsync(c => c.Id == chatId);
        if(chat == null) return null;
        await _dbContext.Entry(chat)
        .Collection(c => c.ChatUsers).Query()
        .Where(cu => cu.User.Connections.Count > 0)
        .LoadAsync();
        List<Connection> connections = new();
        foreach(var cu in chat.ChatUsers)
        {
            connections.AddRange(cu.User.Connections);
        }
        return connections;
    }
}