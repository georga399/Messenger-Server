using Messenger.Models;
using Messenger.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Messenger.Repositories;
public class MessageRepository: IMessageRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    public MessageRepository(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    public async Task Add(Message message)
    {
        await _dbContext.AddAsync(message);
    }
    public async Task<Message?> GetMessageById(int messageId)
    {
        var msg = await _dbContext.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
        return msg;
    }
    public async Task Update(Message message) //TODO: remove this method
    {
        var msg = await _dbContext.Messages.FirstOrDefaultAsync(m => m.Id == message.Id);
        if(msg == null)
        {
            //TODO: Notify about not found
            return;
        }
        msg = message; //
    }
    public async Task Remove(Message message)
    {
        var msg = await _dbContext.Messages.FindAsync(message);
        if(msg == null)
        {
            //TODO: Notify about null
            return;
        }
        _dbContext.Messages.Remove(msg);
    }
    public async Task Remove(int messageId)
    {
        var msg = await _dbContext.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
        if(msg == null)
        {
            //TODO: Notify about null
            return;
        }
        _dbContext.Messages.Remove(msg);
    }
    public async Task<List<Message>?> GetMessagesRangeAsync(int chatId, int messageId, int range) //TODO: TEST IT
    {
        var chat = await _dbContext.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId);
        chat?.Messages.OrderBy(m => m.Id);
        Message? fromMsg;
        if(range > 0)
        {
            fromMsg = chat?.Messages.Where(m => m.Id >= messageId).FirstOrDefault();
        }
        else if(range < 0)
        {
            fromMsg = chat?.Messages.Where(m => m.Id <= messageId).FirstOrDefault();
        }   
        else 
        {
            return new();
        }
        // var fromMsg = chat?.Messages.FirstOrDefault(m => m.Id == messageId);
        if(fromMsg == null)
        {
            // TODO: Notify about it
            return null;
        }
        var fromMsgIndex = fromMsg.Chat!.Messages.FindIndex(m => m == fromMsg);
        if(fromMsgIndex + range >= fromMsg.Chat.Messages.Count || fromMsgIndex + range + 1 < 0) 
        {
            //TODO: Notify about it
            return null;
        }
        List<Message> messages;
        if(range > 0)
            messages = fromMsg.Chat.Messages.GetRange(fromMsgIndex, range); 
        else
            messages = fromMsg.Chat.Messages.GetRange(fromMsgIndex + range, -range);
        return messages;
    }
    public async Task<bool> SetLastReadMessageAsync(string userId, int chatId, int messageId)
    {
        var user = await _dbContext.Users
            .Include(u => u.ChatUsers)
            .ThenInclude(cu => cu.Chat)
            .ThenInclude(c => c.Messages)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if(user == null)
        {
            return false;
        }
        var chatUser = user.ChatUsers.FirstOrDefault(cu => cu.ChatId == chatId);
        if(chatUser == null)
        {
            return false;
        }
        var msg = chatUser.Chat.Messages.FirstOrDefault(m => m.Id == messageId);
        if(msg == null)
        {
            return false;
        }
        chatUser.LastReadMessage = msg;
        return true;
    }
    public async Task<int?> GetNewestMessageIdAsync(int chatId)
    {
        var chat = await _dbContext.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId);
        return chat?.Messages.Max(m => m.Id);
    }
    public async Task<int?> GetLastReadMessageIdAsync(string userId, int chatId)
    {
        var user = await _dbContext.Users
        .Include(u => u.ChatUsers)
        .ThenInclude(cu => cu.LastReadMessageId)
        .FirstOrDefaultAsync(u => u.Id == userId);
        var chatUser = user?.ChatUsers.FirstOrDefault(cu => cu.ChatId == chatId);
        var messageId = chatUser?.LastReadMessageId;
        return messageId;
    }   
    public async Task<Message?> GetMessageByIdInChat(int messageId, int chatId)
    {
        var chat = await _dbContext.Chats
        .Include(c => c.Messages)
        .ThenInclude(m => m.FromUser)
        .FirstOrDefaultAsync(c => c.Id == chatId);
        return chat?.Messages.FirstOrDefault(m => m.Id == messageId);
    }
}