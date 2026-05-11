using BabyShop.Application.Dtos.Auth;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BabyShop.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userRepository.CheckPhoneExistsAsync(dto.PhoneNumber);

        if (existingUser)
            return new AuthResultDto
            {
                Success = false,
                Message = "این شماره موبایل قبلاً ثبت‌نام کرده است"
            };

        var passwordHash = _passwordHasher.HashPassword(dto.Password);
        var verificationCode = GenerateVerificationCode();

        var user = new User(dto.FullName, dto.PhoneNumber, passwordHash);

        if (!string.IsNullOrEmpty(dto.Email))
            user.SetEmail(dto.Email);

        // ذخیره کاربر در دیتابیس
        await _userRepository.CreateUserAsync(user);

        // ذخیره کد تأیید در دیتابیس
        await _userRepository.UpdateVerificationCodeAsync(user.Id, verificationCode);

        return new AuthResultDto
        {
            Success = true,
            Message = "ثبت‌نام با موفقیت انجام شد",
            VerificationCode = verificationCode
        };
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByPhoneNumberAsync(dto.PhoneNumber);

        if (user == null)
            return new AuthResultDto
            {
                Success = false,
                Message = "کاربری با این مشخصات یافت نشد"
            };

        var isPasswordValid = _passwordHasher.VerifyPassword(dto.Password, user.PasswordHash);
        if (!isPasswordValid)
            return new AuthResultDto
            {
                Success = false,
                Message = "رمز عبور نادرست است"
            };

        if (!user.IsPhoneVerified)
        {
            var newCode = GenerateVerificationCode();
            await _userRepository.UpdateVerificationCodeAsync(user.Id, newCode);

            return new AuthResultDto
            {
                Success = false,
                Message = "شماره موبایل تأیید نشده است. کد جدید برای شما ارسال شد.",
                VerificationCode = newCode
            };
        }

        await _userRepository.UpdateLastLoginAsync(user.Id);

        var userDto = new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            IsPhoneVerified = user.IsPhoneVerified
        };

        var token = await GenerateTokenAsync(userDto);

        return new AuthResultDto
        {
            Success = true,
            Message = "ورود موفقیت‌آمیز بود",
            Token = token
        };
    }

    public async Task<AuthResultDto> VerifyPhoneAsync(VerifyPhoneDto dto)
    {
        var user = await _userRepository.GetByPhoneNumberAsync(dto.PhoneNumber);

        if (user == null)
            return new AuthResultDto
            {
                Success = false,
                Message = "کاربر یافت نشد"
            };

        if (user.VerificationCode != dto.Code)
            return new AuthResultDto
            {
                Success = false,
                Message = "کد تأیید نامعتبر است"
            };

        if (user.VerificationCodeExpiry < DateTime.UtcNow)
            return new AuthResultDto
            {
                Success = false,
                Message = "کد تأیید منقضی شده است"
            };

        await _userRepository.VerifyPhoneAsync(user.Id);

        var userDto = new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            IsPhoneVerified = true
        };

        var token = await GenerateTokenAsync(userDto);

        return new AuthResultDto
        {
            Success = true,
            Message = "شماره موبایل با موفقیت تأیید شد",
            Token = token
        };
    }

    public async Task<bool> CheckPhoneExistsAsync(string phoneNumber)
    {
        return await _userRepository.CheckPhoneExistsAsync(phoneNumber);
    }

    public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
    {
        // TODO: پیاده‌سازی Refresh Token
        return new AuthResultDto
        {
            Success = false,
            Message = "Refresh token پیاده‌سازی نشده است"
        };
    }

    private string GenerateVerificationCode()
    {
        return new Random().Next(100000, 999999).ToString();
    }

    private async Task<TokenDto> GenerateTokenAsync(UserDto userDto)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
            new Claim(ClaimTypes.MobilePhone, userDto.PhoneNumber),
            new Claim(ClaimTypes.Name, userDto.FullName),
            new Claim(ClaimTypes.Email, userDto.Email ?? "")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "ThisIsASecretKeyForBabyShopProject2026ThatIsAtLeast32Chars"));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return new TokenDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt,
            RefreshToken = GenerateRefreshToken(),
            User = userDto
        };
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}