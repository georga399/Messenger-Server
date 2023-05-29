using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using AutoMapper;
using Microsoft.AspNetCore.StaticFiles;

using Messenger.Data;
using Messenger.Hubs;
using Messenger.ViewModels;
using Messenger.Models;
using Messenger.Helpers;
using Messenger.Repositories;
namespace Messenger.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController: ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IFileValidator _fileValidator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;

    public UserController(ILogger<UserController> logger,  
        IMapper mapper, IFileValidator fileValidator, IWebHostEnvironment environment,
        IUnitOfWork unitOfWork, UserManager<User> userManager)
    {
        _logger= logger;
        _mapper = mapper;
        _fileValidator = fileValidator;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _environment = environment;
    }
    [HttpGet("getuserinfo/{id}")]
    public IActionResult GetUserInfo(string id)
    {
        var _usr = _unitOfWork.UserRepository.GetById(id);
        if(_usr == null) return BadRequest("Not found");
        UserViewModel usr = _mapper.Map<User, UserViewModel>(_usr);
        return Ok(usr);
    }
    [HttpGet("getuserinfobyname/{userName}")]
    public IActionResult GetUserInfoByName(string userName)
    {
        var _usr = _unitOfWork.UserRepository.GetByName(userName);
        if(_usr == null) return BadRequest("Not found");
        UserViewModel usr = _mapper.Map<User, UserViewModel>(_usr);
        return Ok(usr);
    } 
    [HttpDelete("deleteuser")]
    public async Task<IActionResult> DeleteUser()
    { 
        //TODO: Delete user avatar
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = _unitOfWork.UserRepository.GetById(userId!);
        await _userManager.DeleteAsync(user!);
        return Accepted("deleting user");
    }
    [HttpPost("ava")]
    public async Task<IActionResult> UploadAvatarOfUser(IFormFile file)
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = _unitOfWork.UserRepository.GetById(userId!);
        if(file.FileName.Split('.').Count() <= 1) 
        {
            return BadRequest("Invalid format");
        }
        if(!_fileValidator.IsValidPicture(file))
            return BadRequest("File validation failed!");
        var fileName = userId! + '.' + file.FileName.Split('.').Last();
        var folderPath = Path.Combine(_environment.ContentRootPath, "uploads/usersavatars");
        var filePath = Path.Combine(folderPath, fileName);
        if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        var uploadUri = $"{Request.Scheme}://{Request.Host}/api/user/ava/{fileName}";
        _logger.LogInformation($"Avatar of user uploaded to {uploadUri}");
        user!.Avatar = uploadUri;
        await _unitOfWork.SaveChangesAsync();
        return Accepted(uploadUri);
    }
    [AllowAnonymous]
    [HttpGet("ava/{userId}")]
    public async Task<IActionResult> GetAvatarOfUser(string userId)
    {
        var folderPath = Path.Combine(_environment.ContentRootPath, "uploads/usersavatars");
        var filePath = Path.Combine(folderPath, userId);
        if(filePath == null)
        {
            return BadRequest("File not found");
        }
        // if (!Directory.Exists(filePath))
        // {
        //     return BadRequest("File not found");
        // }
        var provider = new FileExtensionContentTypeProvider();
        if(!provider.TryGetContentType(filePath, out var contenttype))
        {
            contenttype ="application/octet-stream";
        }
        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, contenttype, Path.GetFileName(filePath));
    }

}