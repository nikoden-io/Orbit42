namespace Orback.Application.Dtos;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = null!;

    // Note that refresh token will be used through http-only token, but included here for derived usage (mobile app maybe)
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}