namespace Messenger.ViewModels;
using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required]
    [StringLength(15, ErrorMessage = "Your Password is limited to {2} to {1} characters", MinimumLength = 6)]
    public string Password {get; set;} = null!;

    public string UserName{get; set;} = null!;
}

public class RegisterViewModel :LoginViewModel
{   [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;
}