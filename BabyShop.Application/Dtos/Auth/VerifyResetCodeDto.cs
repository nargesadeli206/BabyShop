using System.ComponentModel.DataAnnotations;

namespace BabyShop.Application.Dtos.Auth;

public class VerifyResetCodeDto
{
    [Required]
    [RegularExpression(@"^09[0-9]{9}$", ErrorMessage = "شماره تلفن نامعتبر است")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "کد باید 6 رقم باشد")]
    public string Code { get; set; } = string.Empty;
}