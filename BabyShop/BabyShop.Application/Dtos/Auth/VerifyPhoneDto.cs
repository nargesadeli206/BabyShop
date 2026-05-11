using System.ComponentModel.DataAnnotations;

namespace BabyShop.Application.Dtos.Auth;

public class VerifyPhoneDto  
{
    [Required]
    [RegularExpression(@"^09[0-9]{9}$")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}