using BabyShop.Application.Dtos.Auth;

namespace BabyShop.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterDto dto);
    Task<AuthResultDto> LoginAsync(LoginDto dto);
    Task<AuthResultDto> VerifyPhoneAsync(VerifyPhoneDto dto);
    Task<bool> CheckPhoneExistsAsync(string phoneNumber);
    Task<AuthResultDto> RefreshTokenAsync(string refreshToken);
}

public class AuthResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public TokenDto? Token { get; set; }
    public string? VerificationCode { get; set; } 
}