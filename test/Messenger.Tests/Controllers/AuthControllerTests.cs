using FakeItEasy;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using AutoMapper; 
using Messenger.Models;
using Messenger.ViewModels;
using Microsoft.Extensions.Logging;

namespace Messenger.Tests.Controllers;
public class AuthControllerTests
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AuthController> _logger;
    private readonly SignInManager<User> _signInManager;
    private readonly IMapper _mapper;
    private AuthController authController;

    public AuthControllerTests()
    {
        _userManager = A.Fake<UserManager<User>>();
        _logger = A.Fake<ILogger<AuthController>>();
        _signInManager = A.Fake<SignInManager<User>>();
        _mapper = A.Fake<IMapper>();
        authController = new AuthController(_userManager, _logger, _signInManager, _mapper);
    }

    [Theory]
    [InlineData("user1")]
    [InlineData("12341")]
    [InlineData("USER@!")]
    public void AuthController_Test_Returns_Status_Ok_and_UserName(string username)
    {
        // Arrange
        authController.ControllerContext.HttpContext = new DefaultHttpContext()
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "<role>")

            }))
        };
        // Act
        var result = authController.Test();
        // Assert
        result.Should().BeOfType(typeof(OkObjectResult));
        result.Equals(username);
    }

    [Fact]
    public async void AuthController_Logout_Returns_Status_Accepted()
    {
        // Arrange
        // Act
        var result = await authController.Logout();
        // Assert
        result.Should().NotBeNull();
        A.CallTo(() => _signInManager.SignOutAsync()).MustHaveHappened();
        result.Should().BeOfType<AcceptedResult>();
    }
    [Theory]
    [InlineData("Password")]
    public async void AuthController_Login_Returns_Status_Ok(string password)
    {
        //Arrange
        var loginViewModel = new LoginViewModel()
        {
            Password = password
        };
        A.CallTo(() => _userManager.CheckPasswordAsync(A<User>.Ignored, password)).Returns(true);
        //Act
        var result = await authController.Login(loginViewModel);
        //Assert
        A.CallTo(() => _signInManager.PasswordSignInAsync(A<string>.Ignored, 
            A<string>.Ignored, A<bool>.Ignored, A<bool>.Ignored)).MustHaveHappened();
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Theory]
    [InlineData("Password")]
    public async void AuthController_Login_Returns_Status_Unauthorized(string password)
    {
        //Arrange
        var loginViewModel = new LoginViewModel()
        {
            Password = password
        };
        A.CallTo(() => _userManager.CheckPasswordAsync(A<User>.Ignored, password)).Returns(false);
        //Act
        var result = await authController.Login(loginViewModel);
        //Assert
        A.CallTo(() => _signInManager.PasswordSignInAsync(A<string>.Ignored, 
            A<string>.Ignored, A<bool>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();
        result.Should().NotBeNull();
        result.Should().BeOfType<UnauthorizedResult>();        
    }
    
    [Theory]
    [InlineData("user1", "user1@mail.com", "QWerty_1111")]
    [InlineData("example", "example@main.com", "p@ssw0rD")]
    public async void AuthController_Register_Returns_Status_Accepted(string username, string email, string password)
    {
        //Arrange
        var registerViewModel = new RegisterViewModel()
        {   
            UserName = username,
            Email = email,
            Password = password
        };
        A.CallTo(() => _userManager.CreateAsync(A<User>.Ignored, password))
            .Returns(IdentityResult.Success);
        //Act
        var result = await authController.Register(registerViewModel);
        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<AcceptedResult>();
    }
}