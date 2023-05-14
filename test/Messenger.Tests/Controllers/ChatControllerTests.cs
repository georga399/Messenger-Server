using Messenger.Repositories;
using Messenger.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

using AutoMapper;
using Messenger.Models;
using Messenger.ViewModels;
namespace Messenger.Tests.Controllers;
public class ChatControllerTests
{
    private readonly ILogger<ChatController> logger;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;
    private readonly IFileValidator fileValidator;
    private readonly IWebHostEnvironment environment;
    private readonly ChatController chatController;
    public ChatControllerTests()
    {
        logger = A.Fake<ILogger<ChatController>>();
        unitOfWork = A.Fake<IUnitOfWork>();
        mapper = A.Fake<IMapper>();
        fileValidator = A.Fake<IFileValidator>();
        environment = A.Fake<IWebHostEnvironment>();
        chatController = new ChatController(logger, mapper, fileValidator, unitOfWork, environment);
        chatController.ControllerContext.HttpContext = new DefaultHttpContext()
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.Role, "<role>"),
                new Claim(ClaimTypes.NameIdentifier, "#1")

            }))
            
        };
    }
    [Fact]
    public async Task ChatController_GetChatInfo_Returns_Ok_With_ChatViewModel()
    {
        //Arrange
        int chatId = 1;
        A.CallTo(() => unitOfWork.ChatRepository.GetChatInfoAsync(chatId))
        .Returns(new Chat(){});
        A.CallTo(() => mapper.Map<Chat, ChatViewModel>(A<Chat>.Ignored))
        .Returns(new ChatViewModel(){});
        //Act
        var result = await chatController.GetChatInfo(chatId);
        //Assert
        result.Should().BeOfType<OkObjectResult>();

    }
    [Fact]
    public async Task ChatController_UploadAvatarOfChat_Returns_Accepted_With_String_URI()
    {
        //Arrange 
        var file = A.Fake<IFormFile>();
        int chatId = 1;
        A.CallTo(() => unitOfWork.ChatRepository.GetChatInfoAsync(chatId))
        .Returns(new Chat(){});
        A.CallTo(() => fileValidator.IsValidPicture(file))
        .Returns(true);
        A.CallTo(() => file.FileName).Returns("random.jpeg");
        A.CallTo(() => unitOfWork.ChatRepository.GetAllMembers(chatId))
        .Returns(new List<ChatUser> 
        {
            new ChatUser()
            {
                UserId = "#1"
            }
        });
        //Act
        var result = await chatController.UploadAvatarOfChat(file, chatId);
        //Assert
        result.Should().BeOfType<AcceptedResult>();
    }
    [Fact]
    public async Task ChatController_GetUsersChats_Returns_Ok()
    {
        //Arrange
        A.CallTo(() => unitOfWork.ChatRepository.GetAllChatsOfUserAsync("userId"))
        .Returns(new List<Chat>());
        //Act
        var result = await chatController.GetUsersChats();
        //Assert
        result.Should().BeOfType<AcceptedResult>();
    }
}