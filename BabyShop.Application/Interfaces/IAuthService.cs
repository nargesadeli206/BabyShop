using BabyShop.Application.Dtos.Auth;

namespace BabyShop.Application.Interfaces.Services;

public interface IAuthService
{
    // ============ متدهای اصلی احراز هویت ============
    Task<AuthResultDto> RegisterAsync(RegisterDto dto);
    Task<AuthResultDto> LoginAsync(LoginDto dto);
    Task<AuthResultDto> VerifyPhoneAsync(VerifyPhoneDto dto);
    Task<bool> CheckPhoneExistsAsync(string phoneNumber);
    Task<AuthResultDto> RefreshTokenAsync(string refreshToken);

    // ============ متدهای فراموشی رمز عبور ============
    Task<AuthResultDto> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<AuthResultDto> VerifyResetCodeAsync(VerifyResetCodeDto dto);
    Task<AuthResultDto> ResetPasswordAsync(ResetPasswordDto dto);
}