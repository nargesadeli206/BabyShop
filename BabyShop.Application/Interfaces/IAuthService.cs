using BabyShop.Application.Dtos.Auth;

namespace BabyShop.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterDto dto);
    Task<AuthResultDto> LoginAsync(LoginDto dto);
    Task<AuthResultDto> VerifyPhoneAsync(VerifyPhoneDto dto);
    Task<bool> CheckPhoneExistsAsync(string phoneNumber);
    Task<AuthResultDto> RefreshTokenAsync(string refreshToken);
}