using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using Messenger.ViewModels;
using Messenger.Data;
using Messenger.Hubs;
using Messenger.Repositories;
using Messenger.Models;
namespace Messenger.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessagesController: ControllerBase
{
    private readonly ILogger<MessagesController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    public MessagesController(ILogger<MessagesController> logger, ApplicationDbContext dbContext, 
        IMapper mapper, IUnitOfWork unitOfWork)
    {
        _logger= logger;
        _dbContext = dbContext;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }
    [HttpGet("getallmessages/chatid={id:int}")]
    public async Task<IActionResult> GetAllMessages(int id)
    {
        var user = await _dbContext.Users
        .Include(u => u.ChatUsers)
        .ThenInclude(cu => cu.Chat)
        .ThenInclude(ch=>ch!.Messages)
        .FirstOrDefaultAsync(u=> u.Id == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        if(user == null) return BadRequest("User not found");
        var chatUser = user.ChatUsers.FirstOrDefault(cu => cu.ChatId == id);
        if(chatUser == null) return BadRequest("Chat not found");
        var messages = _mapper.Map<List<Message>, List<MessageViewModel>>(chatUser.Chat.Messages
            .OrderByDescending(m => m.Timestamp).ToList());
        return Accepted(messages);
    }
    [HttpGet("getmessagesrange/chatid={chatid:int}/frommsgid={messageid:int}-range={range:int}")]
    public async Task<IActionResult> GetMessagesRange(int chatid, int messageid, int range)
    {
        var messages = await _unitOfWork.MessageRepository.GetMessagesRangeAsync(chatid, messageid, range);
        if(messages == null) return BadRequest("Something went wrong");
        var messageViewModels = _mapper.Map<List<Message>, List<MessageViewModel>>(messages
            .OrderBy(m => m.Timestamp).ToList());
        return Ok(messageViewModels);
    }
    [HttpGet("getlastreadmessageid/chatid={chatid}")]
    public async Task<IActionResult> GetLastReadMessageId(int chatid)
    {
        // var user = await _dbContext.Users.Include(u => u.ChatUsers).FirstOrDefaultAsync(u=> u.Id == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var messageId = await _unitOfWork.MessageRepository.GetLastReadMessageIdAsync(userId!, chatid);
        return Ok(messageId);
    }
    [HttpGet("getnewestmessageid/chatid={chatid}")]
    public async Task<IActionResult> GetNewestMessageId(int chatid)
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var messageId = await _unitOfWork.MessageRepository.GetNewestMessageIdAsync(chatid);
        if(messageId == null) return BadRequest("Chat not found");
        return Ok(messageId);    
    }
}