namespace BabyShop.Application.Dtos.Auth;

public class CreateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? VerificationCode { get; set; }
}