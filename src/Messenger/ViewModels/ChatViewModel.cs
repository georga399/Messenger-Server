namespace Messenger.ViewModels;
public class ChatViewModel
{
    public int Id{get; set;}
    public string? Title{get; set;}
    public List<string> UsersId{get; set;} = new();
    public bool IsGroup{get; set;}
    public string? Avatar{get; set;}
    public string? AdminId{get; set;}

}