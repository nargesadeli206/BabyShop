using System.ComponentModel.DataAnnotations;

namespace BabyShop.Application.Dtos.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
    [MinLength(3, ErrorMessage = "نام باید حداقل ۳ کاراکتر باشد")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    [RegularExpression(@"^09[0-9]{9}$", ErrorMessage = "شماره موبایل نامعتبر است")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "ایمیل نامعتبر است")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [MinLength(6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد")]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "رمز عبور و تکرار آن مطابقت ندارند")]
    public string ConfirmPassword { get; set; } = string.Empty;
}