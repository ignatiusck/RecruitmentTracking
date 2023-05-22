using System.ComponentModel.DataAnnotations;

namespace RecruitmentTracking.Models;

public class CandidateSignup
{
    [Display(Name = "Full Name")]
    [Required(ErrorMessage = "Please fill in the required information.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Please fill in the required information.")]
    [EmailAddress(ErrorMessage = "Invalid email address. Please enter a valid email address.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Please fill in the required information.")]
    [RegularExpression(@"^(?=.*?[0-9]).{8,}$", ErrorMessage = "Password must be at least 8 characters long and one digit (0-9).")]
    public string? Password { get; set; }

    [Display(Name = "Confirm Password")]
    [Required(ErrorMessage = "Please fill in the required information.")]
    [Compare("Password", ErrorMessage = "Passwords do not match. Please make sure your password and confirm password entries are identical.")]
    public string? ConfirmPassword { get; set; }
}
