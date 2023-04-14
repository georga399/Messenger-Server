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
    public async Task<IActionResult> DeleteMessage(int chatid, int messageid)
    {
        var user = await _dbContext.Users.Include(u => u.Chats).ThenInclude(ch => ch.Messages).FirstOrDefaultAsync(u=> u.Id == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        if(user == null) return BadRequest("User not found");
        var chat = user.Chats.FirstOrDefault(ch => ch.Id == chatid);
        if(chat == null) return BadRequest("Chat not found");
        var msg = chat.Messages.FirstOrDefault(m => m.InChatId == messageid);
        if(msg == null) return BadRequest("Message not found");
        chat.Messages.Remove(msg);
        await _dbContext.SaveChangesAsync();
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
    [HttpGet("getmessagesrange/chatid={chatid:int}/{from:int}-{to:int}")]
    public async Task<IActionResult> GetMessagesRangeFromTo(int chatid, int from, int to) //TODO: optimize and rewrite for overlap
    { 
        var user = await _dbContext.Users.Include(u => u.Chats).ThenInclude(ch => ch.Messages).FirstOrDefaultAsync(u=> u.Id == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        if(user == null) return BadRequest("User not found");
        var chat = user.Chats.FirstOrDefault(ch => ch.Id == chatid);
        if(chat == null) return BadRequest("Chat not found");
        var fromMsg = chat.Messages.FindIndex(m => m.InChatId == from);
        if(fromMsg == -1) return BadRequest("Undefined range");
        var toMsg = chat.Messages.FindIndex(m => m.InChatId == to);
        if(toMsg == -1) return BadRequest("Undefined range");
        if(fromMsg > toMsg) return BadRequest("Undefined range");
        List<Message> _messages = chat.Messages.GetRange(fromMsg, toMsg-fromMsg+1);
        List<MessageViewModel> messages = _mapper.Map<List<Message>, List<MessageViewModel>>(_messages);
        return Accepted(messages);
    }
    [HttpGet("getmessagesrange/chatid={chatid:int}/{from:int}-count={count:int}")]
    public async Task<IActionResult> GetMessagesRangeCount(int chatid, int from, int count) //TODO: optimize and rewrite for overlap
    { 
        var user = await _dbContext.Users.Include(u => u.Chats).ThenInclude(ch => ch.Messages).FirstOrDefaultAsync(u=> u.Id == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        if(user == null) return BadRequest("User not found");
        var chat = user.Chats.FirstOrDefault(ch => ch.Id == chatid);
        if(chat == null) return BadRequest("Chat not found");
        var fromMsg = chat.Messages.FindIndex(m => m.InChatId == from);
        if(fromMsg == -1) return BadRequest("Undefined range");
        List<Message> _messages = chat.Messages.GetRange(fromMsg, count); // MAYBE UB
        List<MessageViewModel> messages = _mapper.Map<List<Message>, List<MessageViewModel>>(_messages);
        return Accepted(messages);
    }
    [HttpGet("getlastreadmessageid/chatid={chatid}")]
    public async Task<IActionResult> GetLastReadMessageId(int chatid)
    {
        var user = await _dbContext.Users.Include(u => u.ChatUsers).FirstOrDefaultAsync(u=> u.Id == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        if(user == null) return BadRequest("User not found");
        var chatUser = user.ChatUsers.FirstOrDefault(cu => cu.ChatId == chatid);
        if(chatUser == null) return BadRequest("Chat not found");
        var res = new Dictionary<string, int?>(){["chatid"] = chatUser.LastReadMessageInChatId};
        return Accepted(res);
    }
    [HttpGet("getnewestmessageid/chatid={chatid}")]
    public async Task<IActionResult> GetNewestMessageId(int chatid)
    {
        var user = await _dbContext.Users.Include(u => u.ChatUsers).FirstOrDefaultAsync(u=> u.Id == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        if(user == null) return BadRequest("User not found");
        var chatUser = user.ChatUsers.FirstOrDefault(cu => cu.ChatId == chatid);
        if(chatUser == null) return BadRequest("Chat not found");
        var res = new Dictionary<string, int?>(){["chatid"] = chatUser.NewestMessageInChatId};
        return Accepted(res);    
    }
}