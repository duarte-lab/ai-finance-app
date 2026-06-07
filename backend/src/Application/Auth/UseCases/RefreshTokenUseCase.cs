using Application.Auth.DTOs;
using Application.Auth.Interfaces;
using Domain.Entities;

namespace Application.Auth.UseCases;

public class RefreshTokenUseCase
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtGenerator _jwtGenerator;

    public RefreshTokenUseCase(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtGenerator jwtGenerator)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<AuthResponse?> ExecuteAsync(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return null;

        var stored = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (stored is null || !stored.IsActive(DateTime.UtcNow))
            return null;

        var user = await _userRepository.GetByIdAsync(stored.UserId);
        if (user is null)
            return null;

        await _refreshTokenRepository.RevokeAsync(stored, DateTime.UtcNow);

        var newRefresh = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        await _refreshTokenRepository.CreateAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = user.TenantId,
            Token = newRefresh,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30),
        });

        var accessToken = _jwtGenerator.Generate(user.Id, user.TenantId, user.Email, user.Name);
        return new AuthResponse(accessToken, newRefresh, user.Id, user.TenantId, user.Email, user.Name);
    }
}
