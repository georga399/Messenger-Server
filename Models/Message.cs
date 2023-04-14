using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Messenger.Models;
public class Message
{
    public string? Content { get; set;}
    public DateTime Timestamp { get; set;}
    public int FromUserIntId{get; set;}
    public string? FromUserId{get; set;}
    public User? FromUser {get; set;}
    public int ChatId {get; set;}
    [Required]
    public Chat? Chat{get; set;}
    public int InChatId{get; set;}
}