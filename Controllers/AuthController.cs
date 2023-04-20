using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

using Messenger.Models;
using Messenger.ViewModels;
namespace Messenger.Controllers;

[ApiController] //TODO: Email verification
[Route("api/[controller]")]
public class AuthController: ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AuthController> _logger;
    private readonly SignInManager<User> _signInManager;
    private readonly IMapper _mapper;
    public AuthController(UserManager<User> userManager, ILogger<AuthController> logger, SignInManager<User> signInManager, IMapper mapper)
    {
        _userManager = userManager;
        _logger = logger;
        _signInManager = signInManager;
        _mapper = mapper;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var user = new User{UserName = model.Email, Email = model.Email};
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }
            return Ok("Successful registration");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Something Went Wrong in the {nameof(Register)}");
            return Problem($"Something Went Wrong in the {nameof(Register)}", statusCode: 500);
        }
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var user = await _userManager.FindByNameAsync(model.UserName);
        if(user == null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
        {
            return Unauthorized();
        }
        await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
        return Ok("Successful authorization");
    }
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok("Cookies was deleted");
    }

    [HttpGet("test")]
    [Authorize]
    public IActionResult Test()
    {
        
        return Ok(HttpContext.User.FindFirstValue(ClaimTypes.Name));
    }
}