using System.ComponentModel.DataAnnotations;

namespace BabyShop.Application.Dtos.Auth;

public class ResetPasswordDto
{
    [Required]
    [RegularExpression(@"^09[0-9]{9}$", ErrorMessage = "شماره تلفن نامعتبر است")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "کد باید 6 رقم باشد")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "رمز عبور باید حداقل 6 کاراکتر باشد")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword", ErrorMessage = "رمز عبور و تکرار آن مطابقت ندارند")]
    public string ConfirmPassword { get; set; } = string.Empty;
}