using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

using Messenger.ViewModels;
using Messenger.Data;
using Messenger.Hubs;
using Messenger.Models;
namespace Messenger.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")] //TODO: REMOVE CreateGroup JoinChat ENDPOINTS
public class ChatController: ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<MessengerHub> _hubContext;
    private readonly IMapper _mapper;
    public ChatController(ILogger<ChatController> logger, ApplicationDbContext dbContext, IHubContext<MessengerHub> hubContext, IMapper mapper)
    {
        _logger= logger;
        _dbContext = dbContext;
        _hubContext = hubContext;
        _mapper = mapper;
    }
    [HttpGet("getallgroupchats")]
    public IActionResult GetAllGroups()
    {
        var _allGroups = (from chat in _dbContext.Chats.Include(ch => ch.Users) where chat.IsGroup == true select chat).ToList();
        var allGroups = _mapper.Map<List<Chat>, List<ChatViewModel>>(_allGroups);
        return Accepted(allGroups);
    }

    [HttpGet("getchatinfo/chatid={id:int}")]
    public async Task<IActionResult> GetChatInfo(int id)
    {
        var _chat = await _dbContext.Chats.Include(c => c.Users).FirstOrDefaultAsync(ch => ch.Id == id);
        if(_chat == null) return BadRequest("Not found");
        ChatViewModel chat = _mapper.Map<Chat, ChatViewModel>(_chat);
        return Accepted(chat);
    }
    [HttpGet("getuserschats")]
    public async Task<IActionResult> GetUsersChats()
    {
        var user = await _dbContext.Users.Include(u => u.Chats).FirstOrDefaultAsync(u=> u.Id == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        if(user == null) return BadRequest("User not found");
        var chats = _mapper.Map<List<Chat>, List<ChatViewModel>>(user.Chats);
        return Accepted(chats);
    }
    [HttpPut("joingroup/chatid={id:int}")]
    public async Task<IActionResult> JoinGroup(int id) 
    {
        var user = await _dbContext.Users.Include(ch => ch.Chats).FirstOrDefaultAsync(u=> u.Id == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        if(user == null) return BadRequest("User not found");
        var chat = user.Chats.FirstOrDefault(ch => ch.Id == id);
        if(chat != null) return BadRequest("User has already being in this chat");
        chat = await _dbContext.Chats.FirstOrDefaultAsync(ch => ch.Id == id);
        if(chat == null) return BadRequest("Chat not found");
        ChatUser chatUser = new ChatUser{Chat = chat, User = user};
        user.ChatUsers.Add(chatUser);
        user.Chats.Add(chat);
        chat.ChatUsers.Add(chatUser);
        chat.Users.Add(user);
        _dbContext.SaveChanges();
        return Accepted($"Join to group with id {id}");
    }
    [HttpDelete("leavegroupchat/chatid={id:int}")]
    public async Task<IActionResult> LeaveGroup(int id)
    {
        var user = await _dbContext.Users.Include(ch => ch.Chats).FirstOrDefaultAsync(u=> u.Id == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        if(user == null) return BadRequest("User not found");
        Chat? chat = user.Chats.FirstOrDefault(ch => ch.Id == id);
        if(chat == null) return BadRequest("Chat not found");
        user.Chats.Remove(chat);
        await _dbContext.SaveChangesAsync();
        return Accepted($"Leave from group with id {id}");
    }
    [HttpPost("creategroupchat")] 
    public async Task<IActionResult> CreateGroup([FromBody] ChatViewModel chatViewModel) 
    {
        if(!chatViewModel.IsGroup) return BadRequest("Chat should be group");
        Chat chat = _mapper.Map<ChatViewModel, Chat>(chatViewModel);
        await _dbContext.Chats.AddAsync(chat);
        foreach(var usrId in chatViewModel.UsersId)
        {
            var usr = _dbContext.Users.FirstOrDefault(u => u.IntId == usrId);
            if(usr == null)
            {
                _dbContext.Remove(chat);
                return BadRequest($"User with id {usrId} doesn't exist");
            }
            ChatUser cu = new ChatUser{Chat = chat, User = usr};
            usr.ChatUsers.Add(cu);
            usr.Chats.Add(chat);
            chat.ChatUsers.Add(cu);
            chat.Users.Add(usr);
        }
        await _dbContext.SaveChangesAsync();
        return Accepted("Group was created");
    }
}