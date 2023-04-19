namespace Messenger.Models;
public class ChatUser
{
    public string UserId{get; set;} = null!;
    public User? User{get; set;}
    public int ChatId{get; set;}
    public Chat? Chat{get; set;}
    public int? LastReadMessageId{get; set;}    
    public Message? LastReadMessage{get; set;}
    public int? NewestMessageId{get; set;}
    public Message? NewestMessage{get; set;}
}