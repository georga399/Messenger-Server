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
using Messenger.Models;
namespace Messenger.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessagesController: ControllerBase
{
    private readonly ILogger<MessagesController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<MessengerHub> _hubContext;
    private readonly IMapper _mapper;
    public MessagesController(ILogger<MessagesController> logger, ApplicationDbContext dbContext, IHubContext<MessengerHub> hubContext, IMapper mapper)
    {
        _logger= logger;
        _dbContext = dbContext;
        _hubContext = hubContext;
        _mapper = mapper;
    }
    [HttpGet("getallmessages/chatid={id:int}")]
    public async Task<IActionResult> GetAllMessages(int id)
    {
        var user = await _dbContext.Users.Include(ch => ch.Chats).ThenInclude(m=>m.Messages).FirstOrDefaultAsync(u=> u.Id == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        if(user == null) return BadRequest("User not found");
        var chat = user.Chats.FirstOrDefault(ch => ch.Id == id);
        if(chat == null) return BadRequest("Chat not found");
        var messages = _mapper.Map<List<Message>, List<MessageViewModel>>(chat.Messages.OrderByDescending(m => m.Timestamp).ToList());
        return Accepted(messages);
    }
    [HttpDelete("deleteMessage/chatid={chatid}/messageid={messageid:int}")]
    public IActionResult DeleteMessage(int chatid, int messageid)
    {
        return Accepted($"delete messageid={messageid} from chatid={chatid}");
    }
    [HttpPost("sendmessage")]
    public async Task<IActionResult> SendMessage([FromBody] MessageViewModel messageViewModel) 
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u=> u.Id == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        if(user == null) return BadRequest("User not found");
        Message message = _mapper.Map<MessageViewModel, Message>(messageViewModel);
        var chat = await _dbContext.Chats.FirstOrDefaultAsync(ch => ch.Id == message.ChatId);
        if(chat == null) return BadRequest("Chat not found");
        message.FromUser = user;
        message.Chat = chat;
        message.FromUserIntId = user.IntId;
        message.Timestamp = DateTime.UtcNow;
        
        await _dbContext.Messages.AddAsync(message);
        await _dbContext.SaveChangesAsync();
        return Accepted("Sending message");
    }
    [HttpGet("getmessages/chatid={chatid:int}/{from:int}-{to:int}")]
    public IActionResult GetMessageFromTo(int chatid, int from, int to)
    {
        return Accepted($"Get messages of chatid={chatid} from {from} to {to}");
    }
    [HttpGet("getlastreadmessage/chatid={chatid}")]
    public IActionResult GetLastReadMessage(int chatid)
    {
        return Accepted($"get last read message chatid={chatid}");
    }
    [HttpGet("getnewestmessage/chatid={chatid}")]
    public IActionResult GetNewestMessage(int chatid)
    {
        return Accepted($"Get newest message chatid={chatid}");
    }
}