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
public class UploadControllerTests
{
    private readonly UploadController uploadController;
    private readonly IWebHostEnvironment environment;
    private readonly IFileValidator fileValidator;
    private readonly ILogger<UploadController> logger;
    public UploadControllerTests()
    {
        environment = A.Fake<IWebHostEnvironment>();
        fileValidator = A.Fake<IFileValidator>();
        logger = A.Fake<ILogger<UploadController>>();
        uploadController = new UploadController(environment, fileValidator, logger);
        uploadController.ControllerContext.HttpContext = new DefaultHttpContext()
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
    public async Task UploadController_UploadAttachment_Return_Accepted()
    {
        //Arrange
        var file = A.Fake<IFormFile>();
        A.CallTo(() => fileValidator.IsValidMedia(file))
        .Returns(true);
        A.CallTo(() => file.FileName).Returns("random.jpeg");
        //Act
        var result = await uploadController.UploadAttachment(file);
        //Assert
        result.Should().BeOfType<AcceptedResult>();
    }
}