using BabyShop.Core.Entities.Base;
using BabyShop.Core.Exceptions;
using System.Text.RegularExpressions;

namespace BabyShop.Core.Entities;

public class User : BaseEntity
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

 
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();


    private User() { }

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
            throw new BusinessRuleException("نام نمی‌تواند خالی باشد");
        if (fullName.Length < 3)
            throw new BusinessRuleException("نام باید حداقل ۳ کاراکتر باشد");
        FullName = fullName.Trim();
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new BusinessRuleException("شماره موبایل نمی‌تواند خالی باشد");

        // اعتبارسنجی شماره موبایل ایران
        if (!Regex.IsMatch(phoneNumber, @"^09[0-9]{9}$"))
            throw new BusinessRuleException("شماره موبایل نامعتبر است");

        PhoneNumber = phoneNumber;
    }

    public void SetEmail(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new BusinessRuleException("ایمیل نامعتبر است");
            Email = email;
        }
        else
        {
            Email = null;
        }
    }

    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new BusinessRuleException("رمز عبور نامعتبر است");
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
                throw new BusinessRuleException("کد تأیید منقضی شده است");

            IsPhoneVerified = true;
            VerificationCode = null;
            VerificationCodeExpiry = null;
        }
    }

    public void SetLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void AddRole(Role role)
    {
        if (role == null)
            throw new BusinessRuleException("نقش نمی‌تواند خالی باشد");

        if (!UserRoles.Any(ur => ur.RoleId == role.Id))
        {
            UserRoles.Add(new UserRole { UserId = Id, RoleId = role.Id, Role = role });
        }
    }

    public void RemoveRole(int roleId)
    {
        var userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null)
        {
            UserRoles.Remove(userRole);
        }
    }

 
    public bool IsInRole(string roleName)
    {
        return UserRoles.Any(ur => ur.Role.Name == roleName);
    }

    public bool HasPermission(string permissionName)
    {
        // اگه Admin باشه، همه دسترسی‌ها رو داره
        if (IsInRole("Admin"))
            return true;

        return UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Any(rp => rp.Permission.Name == permissionName);
    }

  
    public List<string> GetRoles()
    {
        return UserRoles.Select(ur => ur.Role.Name).ToList();
    }

    public List<string> GetPermissions()
    {
        if (IsInRole("Admin"))
            return GetAllPermissions(); // همه دسترسی‌ها

        return UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();
    }

    private List<string> GetAllPermissions()
    {
        
        return new List<string>
        {
            "ViewProducts", "CreateProduct", "EditProduct", "DeleteProduct",
            "ViewOrders", "CreateOrder", "CancelOrder",
            "ViewUsers", "CreateUser", "EditUser", "DeleteUser",
            "ManageRoles", "ViewPayments", "ManagePayments"
        };
    }
}