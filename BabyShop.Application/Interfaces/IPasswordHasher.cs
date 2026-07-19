namespace BabyShop.Core.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);

    /// <summary>
    /// true اگر هش قدیمی است و بعد از لاگین موفق باید دوباره هش شود.
    /// </summary>
    bool NeedsRehash(string passwordHash);
}