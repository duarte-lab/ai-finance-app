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

    public GoogleSignInUseCase(
        IGoogleTokenValidator googleTokenValidator,
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IJwtGenerator jwtGenerator)
    {
        _googleTokenValidator = googleTokenValidator;
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<AuthResponse?> ExecuteAsync(GoogleAuthRequest request)
    {
        var payload = await _googleTokenValidator.ValidateAsync(request.IdToken);
        if (payload is null)
            return null;

        var user = await _userRepository.GetByGoogleIdAsync(payload.GoogleId);

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

        var token = _jwtGenerator.Generate(user.Id, user.TenantId, user.Email, user.Name);

        return new AuthResponse(token, user.Id, user.TenantId, user.Email, user.Name);
    }
}
