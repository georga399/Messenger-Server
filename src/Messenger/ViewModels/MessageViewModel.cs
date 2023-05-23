namespace Messenger.ViewModels;
public class MessageViewModel
{
    public string Content{get; set;} = null!;
    public int ChatId{get; set;}
    public int Id{get; set;}
    public DateTime Timestamp { get; set;}
    public string? FromUserId{get; set;}
    public string? FromUserName{get; set;}
    public string? AttachUri{get; set;}

}