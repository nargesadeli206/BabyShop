
using BabyShop.Application.Dtos.Auth;
using BabyShop.Application.Interfaces;
using BabyShop.Core.Entities;
using BabyShop.Core.Exceptions;
using BabyShop.Infrastructure.Data; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace BabyShop.Application.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        AppDbContext context,
        IConfiguration configuration,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
    {
        // بررسی وجود شماره موبایل تکراری
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);

        if (existingUser != null)
            return new AuthResultDto { Success = false, Message = "این شماره موبایل قبلاً ثبت‌نام کرده است" };

        // هش کردن رمز عبور
        var passwordHash = _passwordHasher.HashPassword(dto.Password);

        // ایجاد کاربر جدید
        var user = new User(dto.FullName, dto.PhoneNumber, passwordHash);

        if (!string.IsNullOrWhiteSpace(dto.Email))
            user.SetEmail(dto.Email);

        // تولید کد تأیید (برای ارسال پیامک)
        var verificationCode = GenerateVerificationCode();
        user.SetVerificationCode(verificationCode);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // TODO: ارسال پیامک حاوی کد تأیید
        // اینجا می‌تونید سرویس پیامک بزنید (کاوه‌نگار، فاراز، وغیره)

        return new AuthResultDto
        {
            Success = true,
            Message = "ثبت‌نام با موفقیت انجام شد. کد تأیید ارسال شد.",
            VerificationCode = verificationCode // فقط برای تست! در محیط واقعی حذف شود
        };
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);

        if (user == null)
            return new AuthResultDto { Success = false, Message = "کاربری با این مشخصات یافت نشد" };

        // بررسی رمز عبور
        var isPasswordValid = _passwordHasher.VerifyPassword(dto.Password, user.PasswordHash);
        if (!isPasswordValid)
            return new AuthResultDto { Success = false, Message = "رمز عبور نادرست است" };

        // بررسی تأیید شماره موبایل
        if (!user.IsPhoneVerified)
        {
            var newCode = GenerateVerificationCode();
            user.SetVerificationCode(newCode);
            await _context.SaveChangesAsync();

            return new AuthResultDto
            {
                Success = false,
                Message = "شماره موبایل تأیید نشده است. کد جدیدی ارسال شد.",
                VerificationCode = newCode // فقط برای تست
            };
        }

        // به‌روزرسانی آخرین ورود
        user.SetLastLogin();
        await _context.SaveChangesAsync();

        // تولید توکن
        var token = await GenerateTokenAsync(user);

        return new AuthResultDto
        {
            Success = true,
            Message = "ورود موفقیت‌آمیز بود",
            Token = token
        };
    }

    public async Task<AuthResultDto> VerifyPhoneAsync(VerifyPhoneDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);

        if (user == null)
            return new AuthResultDto { Success = false, Message = "کاربر یافت نشد" };

        try
        {
            // بررسی کد (اگر کد اشتباه باشه، DomainException می‌ده)
            if (user.VerificationCode != dto.Code)
                throw new DomainException("کد تأیید نامعتبر است");

            user.VerifyPhone();
            await _context.SaveChangesAsync();

            // تولید توکن بعد از تأیید
            var token = await GenerateTokenAsync(user);

            return new AuthResultDto
            {
                Success = true,
                Message = "شماره موبایل با موفقیت تأیید شد",
                Token = token
            };
        }
        catch (DomainException ex)
        {
            return new AuthResultDto { Success = false, Message = ex.Message };
        }
    }

    public async Task<bool> CheckPhoneExistsAsync(string phoneNumber)
    {
        return await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
    }

    public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
    {
        // پیاده‌سازی Refresh Token در صورت نیاز
        throw new NotImplementedException();
    }

    private string GenerateVerificationCode()
    {
        Random random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private async Task<TokenDto> GenerateTokenAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "ThisIsASecretKeyForBabyShopProject2026"));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddDays(7); // توکن ۷ روزه

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
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                IsPhoneVerified = user.IsPhoneVerified
            }
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