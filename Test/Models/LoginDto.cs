using System.ComponentModel.DataAnnotations;

namespace Test.Models;

public class LoginDto
{
    [RegularExpression(@"^[a-zA-Z0-9]+$", 
        ErrorMessage = "Логин должен содержать только латинские буквы и цифры")]
    public string Login { get; set; }
    [RegularExpression(@"^[a-zA-Z0-9]+$", 
        ErrorMessage = "Логин должен содержать только латинские буквы и цифры")]
    public string Password { get; set; }
}