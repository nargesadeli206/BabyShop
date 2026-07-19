using System.ComponentModel.DataAnnotations;

namespace BabyShop.Application.Dtos.Auth;

public class ForgotPasswordDto
{
    [Required]
    [RegularExpression(@"^09[0-9]{9}$", ErrorMessage = "شماره تلفن نامعتبر است")]
    public string PhoneNumber { get; set; } = string.Empty;
}