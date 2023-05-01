using Microsoft.AspNetCore.Identity;
namespace Messenger.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class User: IdentityUser
{
    // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    // public int IntId{get; set;}
    //Chats where user is Admin
    public List<Chat> AdministrateChats {get; set;} = new();
    // User chats
    public List<ChatUser> ChatUsers{get; set;} = new();
    public string? Avatar{get; set;}
    public List<Connection> Connections{get; set;} = new();
}