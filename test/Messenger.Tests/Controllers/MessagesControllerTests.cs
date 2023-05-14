using Messenger.Repositories;
using Messenger.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Messenger.Tests.Repositories;

using AutoMapper;
using Messenger.Models;
using Messenger.ViewModels;
namespace Messenger.Tests.Controllers;
public class MessagesControllerTests : IDisposable
{
    private readonly ILogger<MessagesController> logger;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;
    private readonly ApplicationDbContext dbContext;
    private readonly MessagesController messagesController;
    public MessagesControllerTests()
    {
        logger = A.Fake<ILogger<MessagesController>>();
        unitOfWork = A.Fake<IUnitOfWork>();
        mapper = A.Fake<IMapper>();
        dbContext = ApplicationDbFactory.GetDbContext();
        messagesController = new MessagesController(logger, dbContext, mapper, unitOfWork);
        messagesController.ControllerContext.HttpContext = new DefaultHttpContext()
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.Role, "<role>"),
                new Claim(ClaimTypes.NameIdentifier, "userId")

            }))
        };
    }
    [Fact]
    public async Task MessagesController_GetMessagesRange_Returns_Ok()
    {
        //Arrange
        int chatId = 1;
        int fromMsg = 1;
        int range = 12;
        A.CallTo(() => unitOfWork.MessageRepository.GetMessagesRangeAsync(chatId, fromMsg, range))
        .Returns(new List<Message>());
        //Act
        var result = await messagesController.GetMessagesRange(chatId, fromMsg, range);
        //Assert
        result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task MessagesController_GetLastReadMessageId_ReturnsOk()
    {
        //Arrange
        int chatId = 1;
        //Act
        var result = await messagesController.GetLastReadMessageId(chatId);
        //Assert
        result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task MessagesController_GetNewestMessageId()
    {
        //Arrange
        int chatId = 1;
        A.CallTo(() => unitOfWork.MessageRepository.GetNewestMessageIdAsync(chatId))
        .Returns(5);
        //Act
        var result = await messagesController.GetNewestMessageId(chatId);
        //Assert
        result.Should().BeOfType<OkObjectResult>();
    }
    public async void Dispose()
    {
        await ApplicationDbFactory.Destroy(dbContext);
    }
}