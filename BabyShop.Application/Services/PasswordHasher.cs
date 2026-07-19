using BabyShop.Core.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace BabyShop.Infrastructure.Services;

/// <summary>
/// هش پسورد با PBKDF2 (نسخه v2).
/// پسوردهای قدیمی SHA256 هنوز verify می‌شوند.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const string VersionPrefix = "v2";

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize);

        return $"{VersionPrefix}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
            return false;

        if (passwordHash.StartsWith(VersionPrefix + ".", StringComparison.Ordinal))
            return VerifyPbkdf2(password, passwordHash);

        return VerifyLegacySha256(password, passwordHash);
    }

    public bool NeedsRehash(string passwordHash)
        => !string.IsNullOrEmpty(passwordHash)
           && !passwordHash.StartsWith(VersionPrefix + ".", StringComparison.Ordinal);

    private static bool VerifyPbkdf2(string password, string stored)
    {
        var parts = stored.Split('.', 3);
        if (parts.Length != 3)
            return false;

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[1]);
            expected = Convert.FromBase64String(parts[2]);
        }
        catch
        {
            return false;
        }

        var actual = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static bool VerifyLegacySha256(string password, string passwordHash)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hash = Convert.ToBase64String(hashedBytes);
        return hash == passwordHash;
    }
}