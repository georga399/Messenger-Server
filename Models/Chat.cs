namespace Messenger.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

public class Chat
{
    public int Id{get; set;}
    public bool IsGroup{get; set;}
    public string? Title{get; set;}
    public List<Message> Messages{get; set;} = new();
    public List<User> Users{get; set;} = new();
    public int CountOfMesages{get; set;}
    public List<ChatUser> ChatUsers{get; set;} = new();
    public string? Avatar{get; set;}
}