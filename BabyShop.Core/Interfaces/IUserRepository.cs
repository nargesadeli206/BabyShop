using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> PhoneExistsAsync(string phoneNumber);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> CheckPhoneExistsAsync(string phoneNumber);
    Task<User> CreateUserAsync(User user);  // پارامتر User می‌گیرد
    Task UpdateVerificationCodeAsync(int userId, string code);
    Task VerifyPhoneAsync(int userId);
    Task UpdateLastLoginAsync(int userId);
}