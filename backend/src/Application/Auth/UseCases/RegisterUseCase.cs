using Application.Auth.DTOs;
using Application.Auth.Interfaces;
using Domain.Entities;

namespace Application.Auth.UseCases;

public class RegisterUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IOwnerPersonProvisioner _ownerPersonProvisioner;

    public RegisterUseCase(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IPasswordHasher passwordHasher,
        IJwtGenerator jwtGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        IOwnerPersonProvisioner ownerPersonProvisioner)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _passwordHasher = passwordHasher;
        _jwtGenerator = jwtGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _ownerPersonProvisioner = ownerPersonProvisioner;
    }

    public async Task<AuthResponse> ExecuteAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request));

        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ArgumentException("Password must have at least 8 characters.", nameof(request));

        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing is not null)
            throw new InvalidOperationException("Email already in use.");

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = $"{request.Name.Trim()}'s Household",
            CreatedAtUtc = DateTime.UtcNow,
        };
        await _tenantRepository.CreateAsync(tenant);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = request.Name.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            TenantId = tenant.Id,
            CreatedAtUtc = DateTime.UtcNow,
        };
        await _userRepository.CreateAsync(user);

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
