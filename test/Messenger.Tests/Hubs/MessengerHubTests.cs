using Microsoft.Extensions.Logging;
using AutoMapper;
using Messenger.Hubs;
using Messenger.Repositories;
using Messenger.ViewModels;
using Microsoft.AspNetCore.SignalR;

using Messenger.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
namespace Messenger.Tests.Hubs;
public class MessengerHubTests
{
    private readonly MessengerHub hub;
    private readonly ILogger<MessengerHub> logger;
    private readonly IMapper mapper;
    private readonly IUnitOfWork unitOfWork;
    public MessengerHubTests()
    {
        logger = A.Fake<ILogger<MessengerHub>>();
        mapper = A.Fake<IMapper>();
        unitOfWork = A.Fake<IUnitOfWork>();
        hub = new MessengerHub(mapper, logger, unitOfWork);
        var hubCallerContext = A.Fake<HubCallerContext>();
        var clients = A.Fake<IHubCallerClients>();
        var callerProxy = A.Fake<ISingleClientProxy>();
        A.CallTo(() => clients.Caller)
        .Returns(callerProxy);
        A.CallTo(() => hubCallerContext.User)
        .Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.Role, "<role>"),
                new Claim(ClaimTypes.NameIdentifier, "userId")

            })));
        hub.Context = hubCallerContext;
        hub.Clients = clients;
    }

    [Fact]
    public async Task MessengerHub_SendMessage_Returns_Success()
    {
        //Arrange
        int chatId = 1;
        var msgVM = new MessageViewModel()
        {
            Content = "SomeContent",
            ChatId = 1
        };
        var msg = new Message()
        {
            Content = "SomeContent",
            ChatId = 1
        };
        A.CallTo(() => mapper.Map<MessageViewModel, Message>(msgVM))
        .Returns(msg);
        A.CallTo(() => mapper.Map<Message, MessageViewModel>(msg))
        .Returns(msgVM);
        var chats = A.Fake<List<Chat>>();
        A.CallTo(() => unitOfWork.ChatRepository.GetAllChatsOfUserAsync(A<string>.Ignored))
        .Returns(new List<Chat>()
        {
            new Chat()
            {
                Id = 1
            }
        });
        //Act
        await hub.SendMessage(chatId, msgVM);
        //Assert
        A.CallTo(() => unitOfWork.MessageRepository.Add(msg))
        .MustHaveHappened();

    }
    [Fact]
    public async Task MessengerHub_DeleteMessage_Returns_Success()
    {
        //Arrange
        int chatId = 1;
        int messageId = 1;
        A.CallTo(() => unitOfWork.ChatRepository.GetAllChatsOfUserAsync(A<string>.Ignored))
        .Returns(new List<Chat>()
        {
            new Chat
            {
                Id = 1,
                IsGroup = true
            }
        });
        A.CallTo(() => unitOfWork.MessageRepository.GetMessageByIdInChat(messageId, chatId))
        .Returns(new Message());
        //Act
        await hub.DeleteMessage(chatId, messageId);
        //Assert
        A.CallTo(() => unitOfWork.MessageRepository.Remove(A<Message>.Ignored))
        .MustHaveHappened();
    }
    [Fact]
    public async Task MessengerHub_CreateChat_Returns_Success()
    {
        //Arrange
        var chatViewModel = new ChatViewModel();
        //Act
        await hub.CreateChat(chatViewModel);
        //Assert
        A.CallTo(()=> unitOfWork.ChatRepository.AddChat(chatViewModel))
        .MustHaveHappened();
    }
    [Fact]
    public async Task MessengerHub_DeleteChat_Returns_Success()
    {
        //Arrange
        int chatId = 1;
        A.CallTo( () => unitOfWork.ChatRepository.GetAllChatsOfUserAsync("userId"))
        .Returns(new List<Chat>()
        {
            new Chat{
                Id = chatId
            }
        });
        //Act
        await hub.DeleteChat(chatId);
        //Assert
        A.CallTo(() => unitOfWork.ChatRepository.DeleteChatIfAdmin(chatId, "userId"))
        .MustHaveHappened();
    }
    [Fact]
    public async Task MessengerHub_JoinChat_Returns_Success()
    {
        //Arrange
        int chatId = 1;
        string userId = "userId";
        A.CallTo(() => unitOfWork.ChatRepository.GetChatInfoAsync(chatId))
        .Returns(new Chat()
        {
            IsGroup = true
        });
        //Act
        await hub.JoinChat(chatId, userId);
        //Assert
        A.CallTo(() => unitOfWork.ChatRepository.JoinChat(chatId, userId))
        .MustHaveHappened();
    }
    [Fact]
    public async Task MessengerHub_LeaveChat_Returns_Success()
    {
        //Arrange
        int chatId = 1;
        A.CallTo(() => unitOfWork.ChatRepository.GetAllChatsOfUserAsync("userId"))
        .Returns(new List<Chat>()
        {
            new Chat
            {
                Id = chatId
            }
        });
        //Act
        await hub.LeaveChat(chatId);
        //Assert
        A.CallTo(() => unitOfWork.ChatRepository.LeaveChat(chatId, "userId"))
        .MustHaveHappened();
    }
    [Fact]
    public async Task MessengerHub_SetLastReadMessage_Returns_Success()
    {
        //Arrange
        int chatId = 1;
        int messageId = 1;
        //Act
        await hub.SetLastReadMessage(chatId, messageId);
        //Assert
        A.CallTo(() => unitOfWork.MessageRepository.SetLastReadMessageAsync("userId", chatId, messageId))
        .MustHaveHappened();
    }
    [Fact]
    public async Task MessengerHub_OnConnectedAsync_AddingToRepository()
    {
        //Arrange
        //Act
        await hub.OnConnectedAsync();
        //Assert
        A.CallTo(() => unitOfWork.ConnectionRepository.Add("userId", A<string>.Ignored));
    }
    [Fact]
    public async Task MessengerHub_OnDisconnectedAsync_AddingToRepository()
    {
        //Arrange
        //Act
        await hub.OnDisconnectedAsync(null);
        //Assert
        A.CallTo(() => unitOfWork.ConnectionRepository.Remove(A<string>.Ignored))
        .MustHaveHappened();
    }
}