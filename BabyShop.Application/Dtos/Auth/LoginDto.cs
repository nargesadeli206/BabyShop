using System.ComponentModel.DataAnnotations;

namespace BabyShop.Application.Dtos.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    [RegularExpression(@"^09[0-9]{9}$", ErrorMessage = "شماره موبایل نامعتبر است")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    public string Password { get; set; } = string.Empty;
}