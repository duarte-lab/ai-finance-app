using Application.Auth.DTOs;
using Application.Auth.Interfaces;
using Domain.Entities;

namespace Application.Auth.UseCases;

public class GoogleSignInUseCase
{
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IOwnerPersonProvisioner _ownerPersonProvisioner;

    public GoogleSignInUseCase(
        IGoogleTokenValidator googleTokenValidator,
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IJwtGenerator jwtGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        IOwnerPersonProvisioner ownerPersonProvisioner)
    {
        _googleTokenValidator = googleTokenValidator;
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _jwtGenerator = jwtGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _ownerPersonProvisioner = ownerPersonProvisioner;
    }

    public async Task<AuthResponse?> ExecuteAsync(GoogleAuthRequest request)
    {
        var payload = await _googleTokenValidator.ValidateAsync(request.IdToken);
        if (payload is null)
            return null;

        var user = await _userRepository.GetByGoogleIdAsync(payload.GoogleId);

        if (user is null)
        {
            user = await _userRepository.GetByEmailAsync(payload.Email);
            if (user is not null && string.IsNullOrWhiteSpace(user.GoogleId))
            {
                user.GoogleId = payload.GoogleId;
                await _userRepository.UpdateAsync(user);
            }
        }

        if (user is null)
        {
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = $"{payload.Name}'s Household"
            };

            await _tenantRepository.CreateAsync(tenant);

            user = new User
            {
                Id = Guid.NewGuid(),
                GoogleId = payload.GoogleId,
                Email = payload.Email,
                Name = payload.Name,
                TenantId = tenant.Id
            };

            await _userRepository.CreateAsync(user);
        }

        await _ownerPersonProvisioner.EnsureOwnerPersonAsync(user);

        var token = _jwtGenerator.Generate(user.Id, user.TenantId, user.Email, user.Name);
        var refreshTokenValue = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        await _refreshTokenRepository.CreateAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = user.TenantId,
            Token = refreshTokenValue,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30),
        });

        return new AuthResponse(token, refreshTokenValue, user.Id, user.TenantId, user.Email, user.Name);
    }
}
