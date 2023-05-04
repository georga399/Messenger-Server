using Messenger.Models;
using Messenger.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Messenger.Repositories;
public class MessageRepository: IMessageRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILogger<MessageRepository> _logger;
    public MessageRepository(ApplicationDbContext dbContext, IMapper mapper,
        ILogger<MessageRepository> logger)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _logger = logger;
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
        if(chat == null)
        {
            return null;
        }
        Message? fromMsg;
        if(range > 0)
        {
            fromMsg = chat?.Messages.Where(m => m.Id >= messageId).FirstOrDefault();
        }
        else if(range < 0)
        {
            fromMsg = chat?.Messages.Where(m => m.Id <= messageId).LastOrDefault();
        }   
        else 
        {
            return new();
        }
        if(fromMsg == null)
        {
            return null;
        }
        var fromMsgIndex = fromMsg.Chat!.Messages.FindIndex(m => m == fromMsg);
        List<Message> messages;
        _logger.LogInformation($"fromMsgIndex={fromMsgIndex}, range={range}");
        if(range > 0)
        {
            int countOfMessages = Math.Min(range, chat!.Messages.Count);
            messages = fromMsg.Chat.Messages.GetRange(fromMsgIndex, countOfMessages); 
        }
        else
        {
            int countOfMessages = Math.Min(-range, chat!.Messages.Count);
            messages = fromMsg.Chat.Messages.GetRange(fromMsgIndex - countOfMessages + 1, countOfMessages);
        }
        return messages;
    }
    public async Task<bool> SetLastReadMessageAsync(string userId, int chatId, int messageId)
    {
        _logger.LogInformation("Setting last read message");
        var user = await _dbContext.Users
            .Include(u => u.ChatUsers)
            .ThenInclude(cu => cu.Chat)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if(user == null)
        {
            return false;
        }
        var chatUser = user.ChatUsers.FirstOrDefault(cu => cu.ChatId == chatId);
        var chat = chatUser?.Chat;
        if(chat == null || chatUser == null)
        {
            return false;
        }
        await _dbContext.Entry(chat).Collection(c => c.Messages).LoadAsync();
        var msg = chat.Messages.FirstOrDefault(m => m.Id == messageId);
        if(msg == null)
        {
            return false;
        }
        chatUser.LastReadMessage = msg;
        chatUser.LastReadMessageId = msg.Id;
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
        .ThenInclude(cu => cu.Chat)
        // .ThenInclude(cu => cu.LastReadMessage)
        .FirstOrDefaultAsync(u => u.Id == userId);
        var chatUser = user?.ChatUsers.FirstOrDefault(cu => cu.ChatId == chatId);
        var messageId = chatUser?.LastReadMessageId;
        if(chatUser == null) return null;
        if(messageId == null)
        {
            var chat = chatUser!.Chat;
            await _dbContext.Entry(chat).Collection(c => c.Messages).LoadAsync();
            var msg = chat?.Messages.Min(m => m.Id);
            _logger.LogInformation($"Message {msg} was setted as last read");
            return msg;
        }
        else
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