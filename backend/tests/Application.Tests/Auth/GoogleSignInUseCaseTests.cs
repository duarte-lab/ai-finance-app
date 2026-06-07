using Application.Auth.DTOs;
using Application.Auth.Interfaces;
using Application.Auth.UseCases;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.Auth;

public class GoogleSignInUseCaseTests
{
    private readonly Mock<IGoogleTokenValidator> _validatorMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IJwtGenerator> _jwtGeneratorMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IOwnerPersonProvisioner> _ownerPersonProvisionerMock = new();

    private GoogleSignInUseCase CreateUseCase() =>
        new(
            _validatorMock.Object,
            _userRepoMock.Object,
            _tenantRepoMock.Object,
            _jwtGeneratorMock.Object,
            _refreshTokenRepositoryMock.Object,
            _ownerPersonProvisionerMock.Object);

    [Fact]
    public async Task Execute_InvalidGoogleToken_ReturnsNull()
    {
        _validatorMock.Setup(x => x.ValidateAsync("bad-token")).ReturnsAsync((GoogleTokenPayload?)null);

        var result = await CreateUseCase().ExecuteAsync(new GoogleAuthRequest("bad-token"));

        result.Should().BeNull();
        _userRepoMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        _tenantRepoMock.Verify(x => x.CreateAsync(It.IsAny<Tenant>()), Times.Never);
    }

    [Fact]
    public async Task Execute_NewUser_CreatesTenantAndUserAndReturnsToken()
    {
        var payload = new GoogleTokenPayload("google-123", "user@example.com", "User Name");
        _validatorMock.Setup(x => x.ValidateAsync("valid-token")).ReturnsAsync(payload);
        _userRepoMock.Setup(x => x.GetByGoogleIdAsync("google-123")).ReturnsAsync((User?)null);
        _jwtGeneratorMock.Setup(x => x.Generate(
            It.IsAny<Guid>(), It.IsAny<Guid>(), "user@example.com", "User Name"))
            .Returns("test-jwt");

        var result = await CreateUseCase().ExecuteAsync(new GoogleAuthRequest("valid-token"));

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("test-jwt");
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.Email.Should().Be("user@example.com");
        result.Name.Should().Be("User Name");
        _tenantRepoMock.Verify(x => x.CreateAsync(It.Is<Tenant>(t => t.Name.Contains("User Name"))), Times.Once);
        _userRepoMock.Verify(x => x.CreateAsync(It.Is<User>(u =>
            u.GoogleId == "google-123" && u.Email == "user@example.com")), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
        _ownerPersonProvisionerMock.Verify(x => x.EnsureOwnerPersonAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ExistingUser_DoesNotCreateTenantAndReturnsToken()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            GoogleId = "google-456",
            Email = "existing@example.com",
            Name = "Existing User",
            TenantId = tenantId,
        };

        var payload = new GoogleTokenPayload("google-456", "existing@example.com", "Existing User");
        _validatorMock.Setup(x => x.ValidateAsync("valid-token")).ReturnsAsync(payload);
        _userRepoMock.Setup(x => x.GetByGoogleIdAsync("google-456")).ReturnsAsync(existingUser);
        _jwtGeneratorMock.Setup(x => x.Generate(userId, tenantId, "existing@example.com", "Existing User"))
            .Returns("existing-jwt");

        var result = await CreateUseCase().ExecuteAsync(new GoogleAuthRequest("valid-token"));

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("existing-jwt");
        result.UserId.Should().Be(userId);
        result.TenantId.Should().Be(tenantId);
        _tenantRepoMock.Verify(x => x.CreateAsync(It.IsAny<Tenant>()), Times.Never);
        _userRepoMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
        _ownerPersonProvisionerMock.Verify(x => x.EnsureOwnerPersonAsync(existingUser), Times.Once);
    }
}
