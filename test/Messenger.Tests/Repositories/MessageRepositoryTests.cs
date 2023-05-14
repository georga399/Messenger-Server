using Messenger.Repositories;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using Messenger.Models;
namespace Messenger.Tests.Repositories;
public class MessageRepositoryTests: IDisposable
{
    protected readonly ApplicationDbContext dbContext;
    protected readonly MessageRepository messageRepository;
    protected readonly ILogger<MessageRepository> logger;
    protected readonly IMapper mapper;
    public MessageRepositoryTests()
    {
        dbContext = ApplicationDbFactory.GetDbContext();
        logger = A.Fake<ILogger<MessageRepository>>();
        mapper = A.Fake<IMapper>();
        messageRepository = new MessageRepository(dbContext, mapper, logger);
    }
    
    [Fact]
    public async void MessageRepository_GetMessageById_Returns_MessageObj()
    {
        //Arrange
        int msgId = 1;
        //Act
        var result = await messageRepository.GetMessageById(msgId);
        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Message>();    
    }
    [Fact]
    public async Task MessageRepository_Remove()
    {
        //Arrange
        int msgId = 1;
        var msg = dbContext.Messages.FirstOrDefault(m => m.Id == msgId);
        //Act
        await messageRepository.Remove(msg!);
        dbContext.SaveChanges();
        //Assert
        dbContext.Messages.Should().NotContain(m => m.Id == msgId);
    }

    [Theory]
    [InlineData(1, 1, 5)]
    [InlineData(1, 1, 10)]
    [InlineData(1, 0, 20)]
    [InlineData(1, 1000, -10)]
    [InlineData(1, 1000, -1)]
    [InlineData(1, -1000, 1)]
    public async Task MessageRepository_GetMessagesRange_Returns_ListOfMsgs(int chatId, int messageId, int range)
    {
        //Arrange

        //Act
        var result = await messageRepository.GetMessagesRangeAsync(chatId, messageId, range);
        //Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }
    [Theory]
    [InlineData(1, 1000, 1)]
    [InlineData(20, 0, 1)]
    [InlineData(1, -5, -10)]
    public async Task MessageRepository_GetMessagesRange_Returns_Null(int chatId, int messageId, int range)
    {
        //Arrange

        //Act
        var result = await messageRepository.GetMessagesRangeAsync(chatId, messageId, range);
        //Assert
        result.Should().BeNull();
    }
    public async void Dispose()
    {
        await ApplicationDbFactory.Destroy(dbContext);
    }
    
    [Theory]
    [InlineData("#101", 1)]
    public async Task MessageRepository_GetLastReadMessage_Returns_Null(string userId, int chatId)
    {
        //Arrange

        //Act
        var result = await messageRepository.GetLastReadMessageIdAsync(userId, chatId);
        //Assert
        result.Should().BeNull();
    }
    [Theory]
    [InlineData("#1", 1)]
    [InlineData("#2", 2)]
    [InlineData("#3", 3)]
    public async Task MessageRepository_GetLastReadMessage_Returns_Int(string userId, int chatId)
    {
        //Arrange

        //Act
        var result = await messageRepository.GetLastReadMessageIdAsync(userId, chatId);
        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType(typeof(int));
    }
    [Theory]
    [InlineData("#1", 1, 1)]
    [InlineData("#1", 1, 5)]
    [InlineData("#2", 1, 5)]
    [InlineData("#3", 2, 11)]
    public async Task MessageRepository_SetLastReadMessageId_Returns_True(string userId, int chatId, int messageId)
    {
        //Arrange

        //Act
        var result = await messageRepository.SetLastReadMessageAsync(userId, chatId, messageId);
        //Assert
        result.Should().BeTrue();
        var chat = dbContext.Chats
        .Include(ch => ch.ChatUsers)
        .ThenInclude(cu => cu.LastReadMessage)
        .FirstOrDefault(ch => ch.Id == chatId);
        chat!.ChatUsers
        .FirstOrDefault(cu => cu.UserId == userId)!
        .LastReadMessage!.Id.Should().Be(messageId);
    }
    [Theory]
    [InlineData("#1", 1, 11)]
    [InlineData("#1", 1, 12341)]
    [InlineData("#", 1, 5)]
    [InlineData("#3", 125, 11)]
    public async Task MessageRepository_SetLastReadMessageId_Returns_False(string userId, int chatId, int messageId)
    {
        //Arrange

        //Act
        var result = await messageRepository.SetLastReadMessageAsync(userId, chatId, messageId);
        //Assert
        result.Should().BeFalse();
    }
}