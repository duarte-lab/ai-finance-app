using Domain.Entities;

namespace Application.Auth.Interfaces;

public interface IRefreshTokenRepository
{
    Task CreateAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RevokeAsync(RefreshToken refreshToken, DateTime revokedAtUtc);
}
