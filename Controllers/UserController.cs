using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using AutoMapper;

using Messenger.Data;
using Messenger.Hubs;
using Messenger.ViewModels;
using Messenger.Models;
namespace Messenger.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController: ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<MessengerHub> _hubContext;
    private readonly IMapper _mapper;
    public UserController(ILogger<UserController> logger, ApplicationDbContext dbContext, IHubContext<MessengerHub> hubContext, IMapper mapper)
    {
        _logger= logger;
        _dbContext = dbContext;
        _hubContext = hubContext;
        _mapper = mapper;
    }
    [HttpGet("getuserinfo/{id}")]
    public IActionResult GetUserInfo(int id)
    {
        var _usr = _dbContext.Users.FirstOrDefault(u => u.IntId == id);
        if(_usr == null) return BadRequest("Not found");
        UserViewModel usr = _mapper.Map<User, UserViewModel>(_usr);
        return Accepted(usr);
    } 
    [HttpPut("edituserinfo")]
    public IActionResult EditUserInfo([FromBody] UserViewModel userViewModel)
    {
        return Accepted(userViewModel);
    }
    [HttpDelete("deleteuser")]
    public IActionResult DeleteUser()
    {
        return Accepted("deleting user");
    }
}