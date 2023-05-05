using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.StaticFiles;

using Messenger.ViewModels;
using Messenger.Data;
using Messenger.Hubs;
using Messenger.Models;
using Messenger.Helpers;
using Messenger.Repositories;
namespace Messenger.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")] 
public class ChatController: ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<MessengerHub> _hubContext;
    private readonly IMapper _mapper;
    private readonly IFileValidator _fileValidator;
    private readonly IWebHostEnvironment _environment;

    public ChatController(ILogger<ChatController> logger, ApplicationDbContext dbContext, 
        IHubContext<MessengerHub> hubContext, IMapper mapper,
        IFileValidator fileValidator, IUnitOfWork unitOfWork, IWebHostEnvironment environment)
    {
        _logger= logger;
        _dbContext = dbContext;
        _hubContext = hubContext;
        _mapper = mapper;
        _fileValidator = fileValidator;
        _unitOfWork = unitOfWork;
        _environment = environment;
    }
    [HttpGet("getchatinfo/chatid={id:int}")]
    public async Task<IActionResult> GetChatInfo(int id)
    {
        var _chat = await _unitOfWork.ChatRepository.GetChatInfoAsync(id);
        if(_chat == null) return BadRequest("Not found");
        ChatViewModel chat = _mapper.Map<Chat, ChatViewModel>(_chat);
        return Accepted(chat);
    }
    [HttpGet("getuserschats")]
    public async Task<IActionResult> GetUsersChats()
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var chats = await _unitOfWork.ChatRepository.GetAllChatsOfUserAsync(userId!);
        if(chats == null) return BadRequest("User not found");
        var chatViewModels = _mapper.Map<List<Chat>, List<ChatViewModel>>(chats);
        return Accepted(chatViewModels);
    }
    [HttpPost("chatava/{chatId:int}")]
    public async Task<IActionResult> UploadAvatarOfChat(IFormFile file, int chatId)
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var chat = await _unitOfWork.ChatRepository.GetChatInfoAsync(chatId);
        if(chat == null)
        {
            return BadRequest("Chat not found");

        }
        var chatUsers = await _unitOfWork.ChatRepository.GetAllMembers(chatId);
        var cu = chatUsers!.FirstOrDefault(c => c.UserId == userId);
        if(cu == null)
        {
            return BadRequest("Chat not found");
        }
        if(!_fileValidator.IsValidPicture(file))
            return BadRequest("File validation failed!");
        var fileName = chat.Id.ToString() + '.' + file.FileName.Split('.')[1];
        _logger.LogInformation($"Set avatar for chat {fileName}");
        var folderPath = Path.Combine(_environment.ContentRootPath, "uploads/chatsavatars");
        var filePath = Path.Combine(folderPath, fileName);
        if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        var uploadUri = $"{Request.Scheme}://{Request.Host}/api/chat/ava/{fileName}";
        chat!.Avatar = uploadUri;
        await _unitOfWork.SaveChangesAsync();
        return Accepted(uploadUri);
    }
    [HttpGet("chatava/{avatar}")]
    public async Task<IActionResult> GetAvatarOfChat(string avatar)
    {
        var folderPath = Path.Combine(_environment.ContentRootPath, "uploads/chatsavatars");
        var filePath = Path.Combine(folderPath, avatar);
        if(filePath == null)
        {
            return BadRequest("File not found");
        }
        var provider = new FileExtensionContentTypeProvider();
        if(!provider.TryGetContentType(filePath, out var contenttype))
        {
            contenttype ="application/octet-stream";
        }
        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, contenttype, Path.GetFileName(filePath));
    }
}