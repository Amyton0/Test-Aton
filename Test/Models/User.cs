using System.ComponentModel.DataAnnotations;

namespace Test.Models;

public class User
{
    [Key]
    public Guid Guid { get; set; }
    [RegularExpression(@"^[a-zA-Z0-9]+$", 
        ErrorMessage = "Логин должен содержать только латинские буквы и цифры")]
    public string Login { get; set; }
    [RegularExpression(@"^[a-zA-Z0-9]+$", 
        ErrorMessage = "Логин должен содержать только латинские буквы и цифры")]
    public string Password { get; set; }
    [Required]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ]+$",
        ErrorMessage = "Имя должно содержать только русские и латинские буквы")]
    public string Name { get; set; }
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public bool Admin { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? RevokedOn { get; set; }
    public string? RevokedBy { get; set; }
}