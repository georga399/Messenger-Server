using Messenger.Repositories;
using Messenger.Models;
using Messenger.ViewModels;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Messenger.Tests.Repositories;
public class ChatRepositoryTest: IDisposable
{
    protected readonly ApplicationDbContext dbContext;
    protected readonly ChatRepository chatRepository;
    protected readonly IMapper mapper;
    protected readonly ILogger<ChatRepository> logger;
    public ChatRepositoryTest()
    {
        dbContext = ApplicationDbFactory.GetDbContext();
        mapper = A.Fake<IMapper>();
        logger = A.Fake<ILogger<ChatRepository>>();
        chatRepository = new ChatRepository(dbContext, mapper, logger);
    }
    [Fact]
    public void ChatRepository_AddChat_Returns_ChatObj()
    {
        //Arrange
        var chatViewModel = new ChatViewModel()
        {
            Title = "SomeTitle",
            IsGroup = true,
            AdminId = "#1",
            UsersId = new()
            {
                "#1", "#2", "#3"
            }
        };
        var chat = new Chat()
        {
            Title = "SomeTitle",
            IsGroup = true,
            AdminId = "#1"
        };
        A.CallTo(() => mapper.Map<ChatViewModel, Chat>(chatViewModel)).Returns(chat);
        //Act
        var result = chatRepository.AddChat(chatViewModel);
        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Chat>();
        result!.Admin.Should().Be(dbContext.Users.FirstOrDefault(u => u.Id == "#1"));
        result.ChatUsers.Select(cu => cu.UserId).Should().Equal(chatViewModel.UsersId);
    }

    [Fact]
    public void ChatRepository_AddChat_Return_Null()
    {
        //Arrange
        var chatViewModel = new ChatViewModel()
        {
            Title = "SomeTitle",
            IsGroup = true,
            AdminId = "",
            UsersId = new()
            {
                "#1", "#2", "#3"
            }
        };
        var chat = new Chat()
        {
            Title = "SomeTitle",
            IsGroup = true,
            AdminId = ""
        };
        A.CallTo(() => mapper.Map<ChatViewModel, Chat>(chatViewModel)).Returns(chat);

        //Act
        var result1 = chatRepository.AddChat(chatViewModel);
        //Assert
        result1.Should().BeNull();

    }
    [Fact]
    public async Task ChatRepository_Remove_Returns_True()
    {
        //Arrange
        int chatId = 1;
        //Act
        var result = await chatRepository.Remove(chatId);
        //Assert
        result.Should().Be(true);
    }
    [Fact]
    public async Task ChatRepository_Remove_Returns_Null()
    {
        //Arrange
        int chatId = 0;
        //Act
        var result = await chatRepository.Remove(chatId);
        //Assert
        result.Should().Be(false);
    }
    [Fact]
    public async Task ChatRepository_JoinChat_Returns_True()
    {
        //Arrange
        int chatId = 1;
        string userId = "#101";
        //Act
        var result = await chatRepository.JoinChat(chatId, userId);
        //Assert
        result.Should().BeTrue();
    }
    [Theory]
    [InlineData(0, "")]
    [InlineData(1, "#1")]
    public async Task ChatRepository_JoinChat_Returns_False(int chatId, string userId)
    {
        //Arrange
        //Act
        var result = await chatRepository.JoinChat(chatId, userId);
        //Assert
        result.Should().Be(false);
    }
    public async void Dispose()
    {
        await ApplicationDbFactory.Destroy(dbContext);
    }

    [Fact]
    public async Task ChatRepository_LeaveChat_Returns_True_With_Removing_Chat()
    {
        //Arrange
        int chatId = 1;
        string userId = "#1";
        //Act
        var result = await chatRepository.LeaveChat(chatId, userId);
        dbContext.SaveChanges();
        //Assert
        result.Should().BeTrue();
        dbContext.Chats.ToList().Should().NotContain( o => o.Id == chatId);
    }
    [Theory]
    [InlineData(1, "#2")]
    // [InlineData(1, "#101")]
    public async Task ChatRepository_SetAdmin_True(int chatId, string adminId)
    {
        //Arrange
        var prevAdminId = dbContext.Chats
        .Include(c => c.Admin)
        .FirstOrDefault(c => c.Id == chatId)!.Admin!.Id;
        //Act
        var result = await chatRepository.SetAdmin(chatId, adminId);
        dbContext.SaveChanges();
        //Assert
        result.Should().BeTrue();
            //Check previous admin 
        dbContext.Users
        .Include(a => a.AdministrateChats)
        .FirstOrDefault(u => u.Id == prevAdminId)!
        .AdministrateChats.Should().NotContain(o => o.Id == chatId);
            //Check newest admin
        var chat = dbContext.Chats
        .Include(c => c.Admin)
        .FirstOrDefault(c => c.Id == chatId);
        chat!.AdminId.Should().Be(adminId);
        dbContext.Users
        .Include(a => a.AdministrateChats)
        .FirstOrDefault(u => u.Id == adminId)!
        .AdministrateChats.Should().Contain(o => o.Id == chatId);
    }
    [Theory]
    [InlineData(1, "#1")]
    [InlineData(2, "#2")]
    [InlineData(1, "#101")]
    public async Task ChatRepository_SetAdmin_Returns_False(int chatId, string adminId)
    {
        //Arrange
        //Act
        var result = await chatRepository.SetAdmin(chatId, adminId);
        //Assert
        result.Should().BeFalse();
    }
    [Fact]
    public async Task ChatRepository_DeleteChatIfAdmin_Returns_True()
    {
        //Arrange
        int chatId = 1;
        string adminId = "#1";
        //Act
        var result = await chatRepository.DeleteChatIfAdmin(chatId, adminId);
        dbContext.SaveChanges();
        //Assert
        result.Should().BeTrue();
            //Check that chat was deleted
        dbContext.Chats.ToList().Should().NotContain(ch => ch.Id == chatId);
        dbContext.Users
        .Include(u => u.ChatUsers).FirstOrDefault(u => u.Id == adminId)!
        .ChatUsers.Should().NotContain(ch => ch.ChatId == chatId);
    }
    [Theory]
    [InlineData(1, "#2")]
    [InlineData(1233, "#1")]
    [InlineData(1, "14412")]
    public async Task ChatRepository_DeleteChatIfAdmin_Returns_False(int chatId, string userId)
    {
        //Act
        var res = await chatRepository.DeleteChatIfAdmin(chatId, userId);
        //Assert
        res.Should().BeFalse();
    }
}