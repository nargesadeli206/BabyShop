namespace BabyShop.Application.Dtos.Auth;

public class AuthResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public TokenDto? Token { get; set; }
    public string? VerificationCode { get; set; }
}