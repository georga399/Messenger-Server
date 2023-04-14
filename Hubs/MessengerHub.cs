using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization; 
using AutoMapper;
using Messenger.Models;
using Messenger.ViewModels;
using Messenger.Data;  
namespace Messenger.Hubs;

[Authorize]
public class MessengerHub: Hub
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public MessengerHub(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task SendMessage(int chatId, string message)
    {
        await this.Clients.All.SendAsync("Recieve", message);
    }
    
    // public async Task DeleteMessage(int chatId, int inChatId)
    // {
        
    // }
    // public async Task Join(int chatId, string userId)
    // {

    // }
    // public async Task Leave(int chatId)
    // {
        
    // }

}