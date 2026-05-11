using BabyShop.Application.Dtos.Auth;

namespace BabyShop.Application.Dtos.Auth;

public class TokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}