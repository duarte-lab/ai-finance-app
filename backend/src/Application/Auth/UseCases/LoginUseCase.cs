using Application.Auth.DTOs;
using Application.Auth.Interfaces;
using Domain.Entities;

namespace Application.Auth.UseCases;

public class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IOwnerPersonProvisioner _ownerPersonProvisioner;

    public LoginUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtGenerator jwtGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        IOwnerPersonProvisioner ownerPersonProvisioner)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtGenerator = jwtGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _ownerPersonProvisioner = ownerPersonProvisioner;
    }

    public async Task<AuthResponse?> ExecuteAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
            return null;

        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
            return null;

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        await _ownerPersonProvisioner.EnsureOwnerPersonAsync(user);

        var accessToken = _jwtGenerator.Generate(user.Id, user.TenantId, user.Email, user.Name);
        var refreshToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        await _refreshTokenRepository.CreateAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = user.TenantId,
            Token = refreshToken,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30),
        });

        return new AuthResponse(accessToken, refreshToken, user.Id, user.TenantId, user.Email, user.Name);
    }
}
