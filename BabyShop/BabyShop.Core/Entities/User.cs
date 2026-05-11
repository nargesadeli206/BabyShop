using BabyShop.Core.Exceptions;
using System.Net;

namespace BabyShop.Core.Entities;

public class User : Basket
{
    public string FullName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsPhoneVerified { get; private set; }
    public string? VerificationCode { get; private set; }
    public DateTime? VerificationCodeExpiry { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public ICollection<Order>? Orders { get; set; }

    private User() { } // برای EF Core

    public User(string fullName, string phoneNumber, string passwordHash)
    {
        SetFullName(fullName);
        SetPhoneNumber(phoneNumber);
        SetPassword(passwordHash);
        IsPhoneVerified = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("نام نمی‌تواند خالی باشد");
        if (fullName.Length < 3)
            throw new DomainException("نام باید حداقل ۳ کاراکتر باشد");
        FullName = fullName.Trim();
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("شماره موبایل نمی‌تواند خالی باشد");

        // Validation ساده شماره موبایل ایران
        if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^09[0-9]{9}$"))
            throw new DomainException("شماره موبایل نامعتبر است");

        PhoneNumber = phoneNumber;
    }

    public void SetEmail(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new DomainException("ایمیل نامعتبر است");
            Email = email;
        }
    }

    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("رمز عبور نامعتبر است");
        PasswordHash = passwordHash;
    }

    public void SetVerificationCode(string code)
    {
        VerificationCode = code;
        VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(5);
    }

    public void VerifyPhone()
    {
        if (!IsPhoneVerified)
        {
            if (VerificationCodeExpiry < DateTime.UtcNow)
                throw new DomainException("کد تأیید منقضی شده است");

            IsPhoneVerified = true;
            VerificationCode = null;
            VerificationCodeExpiry = null;
        }
    }

    public void SetLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}