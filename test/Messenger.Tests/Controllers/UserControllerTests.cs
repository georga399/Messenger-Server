using Messenger.Controllers;
using Messenger.Repositories;
using Messenger.Models;
using Messenger.Helpers;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Messenger.ViewModels;
namespace Messenger.Tests.Controllers;
public class UserControllerTests
{
    private readonly UserController userController;
    private readonly ILogger<UserController> logger;
    private readonly UserManager<User> userManager;
    private readonly IMapper mapper;
    private readonly IFileValidator fileValidator;
    private readonly IUnitOfWork unitOfWork;
    private readonly IWebHostEnvironment environment;
    public UserControllerTests()
    {
        userManager = A.Fake<UserManager<User>>();
        logger = A.Fake<ILogger<UserController>>();
        mapper = A.Fake<IMapper>();
        unitOfWork = A.Fake<IUnitOfWork>();
        environment = A.Fake<IWebHostEnvironment>();  
        fileValidator = A.Fake<IFileValidator>();     
        userController = new UserController(logger, mapper, fileValidator, environment, unitOfWork, userManager);
        userController.ControllerContext.HttpContext = new DefaultHttpContext()
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
    public void UserController_GetUserInfo_Returns_OkObjResult()
    {
        //Arrange
        var userId = "#1";
        var user = new User();
        A.CallTo(() => unitOfWork.UserRepository.GetById(userId))
        .Returns(user);
        var userViewModel = new UserViewModel();
        A.CallTo(() => mapper.Map<User, UserViewModel>(user))
        .Returns(userViewModel);
        //Act
        var result = userController.GetUserInfo(userId);
        //Asssert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task UserController_DeleteUser_Returns_Accepted()
    {
        //Arrange
        A.CallTo(() => unitOfWork.UserRepository.GetById("userId"))
        .Returns(new User());
        //Act
        var result = await userController.DeleteUser();
        //Assert
        A.CallTo(() => userManager.DeleteAsync(A<User>.Ignored))
        .MustHaveHappened();
        result.Should().BeOfType<AcceptedResult>();
    }

    [Fact]
    public async Task UserController_UploadAvatarOfUser_Returns_Accepted()
    {
        //Arrange
        var file = A.Fake<IFormFile>();
        A.CallTo(() => fileValidator.IsValidPicture(file))
        .Returns(true);
        A.CallTo(() => file.FileName).Returns("random.jpeg");
        //Act
        var result = await userController.UploadAvatarOfUser(file);
        //Assert
        result.Should().BeOfType<AcceptedResult>();
    }
}