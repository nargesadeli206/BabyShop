using BabyShop.Core.Entities;
using System.Linq.Expressions;

namespace BabyShop.Core.Interfaces;

// بدون : IRepository
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<IReadOnlyList<User>> GetAllAsync();
    Task<IReadOnlyList<User>> GetPagedAsync(int page, int pageSize);
    Task<User> AddAsync(User entity);
    Task UpdateAsync(User entity);
    Task DeleteAsync(User entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();
    Task<IReadOnlyList<User>> FindAsync(Expression<Func<User, bool>> predicate);
    Task<User?> FirstOrDefaultAsync(Expression<Func<User, bool>> predicate);

    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> PhoneExistsAsync(string phoneNumber);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> CheckPhoneExistsAsync(string phoneNumber);
    Task<User> CreateUserAsync(User user);
    Task UpdateVerificationCodeAsync(int userId, string code);
    Task VerifyPhoneAsync(int userId);
    Task UpdateLastLoginAsync(int userId);
    Task<IReadOnlyList<string>> GetUserRoleNamesAsync(int userId);
    Task UpdatePasswordAsync(int userId, string passwordHash);
}