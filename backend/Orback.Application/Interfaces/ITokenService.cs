using Orback.Domain.Entities;

namespace Orback.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user);
    string GenerateRefreshTokenString();
}