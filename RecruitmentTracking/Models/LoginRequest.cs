using System.ComponentModel.DataAnnotations;

namespace RecruitmentTracking.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "Please fill in the required information.")]
    [EmailAddress(ErrorMessage = "Invalid email address. Please enter a valid email address.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Please fill in the required information.")]
    [RegularExpression(@"^(?=.*?[0-9]).{8,}$", ErrorMessage = "Password must be at least 8 characters long and one digit (0-9).")]
    public string? Password { get; set; }
}
