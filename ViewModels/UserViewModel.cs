namespace Messenger.ViewModels;
public class UserViewModel
{
    public int Id{get; set;}
    public string UserName{get; set;} = null!;
    public string Email{get; set;} = null!;
    public string? Avatar{get; set;}
}