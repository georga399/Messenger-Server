namespace Messenger.ViewModels;
public class UserViewModel
{
    public string Id{get; set;} = null!;
    public string UserName{get; set;} = null!;
    public string Email{get; set;} = null!;
    public string? Avatar{get; set;}
}