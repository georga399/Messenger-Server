namespace Messenger.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

public class Chat
{
    public int Id{get; set;}
    public bool IsGroup{get; set;}
    public string? Title{get; set;}
    public List<Message> Messages{get; set;} = new();
    public List<ChatUser> ChatUsers{get; set;} = new();
    public string? Avatar{get; set;}
    public string? AdminId { get; set; }
    public User? Admin{get; set;}
}