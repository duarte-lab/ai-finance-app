using Application.Auth.DTOs;
using Application.Auth.Interfaces;
using Application.Auth.UseCases;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.Auth;

public class AuthUseCasesTests
{
    [Fact]
    public async Task Register_NewUser_ShouldCreateUserAndOwnerAndTokens()
    {
        var userRepo = new Mock<IUserRepository>();
        var tenantRepo = new Mock<ITenantRepository>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var jwtGenerator = new Mock<IJwtGenerator>();
        var refreshRepo = new Mock<IRefreshTokenRepository>();
        var ownerProvisioner = new Mock<IOwnerPersonProvisioner>();

        userRepo.Setup(x => x.GetByEmailAsync("ana@example.com")).ReturnsAsync((User?)null);
        passwordHasher.Setup(x => x.Hash("password-123")).Returns("hashed-pwd");
        jwtGenerator.Setup(x => x.Generate(It.IsAny<Guid>(), It.IsAny<Guid>(), "ana@example.com", "Ana"))
            .Returns("jwt-token");

        var useCase = new RegisterUseCase(
            userRepo.Object,
            tenantRepo.Object,
            passwordHasher.Object,
            jwtGenerator.Object,
            refreshRepo.Object,
            ownerProvisioner.Object);

        var result = await useCase.ExecuteAsync(new RegisterRequest("Ana", "ana@example.com", "password-123"));

        result.AccessToken.Should().Be("jwt-token");
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        userRepo.Verify(x => x.CreateAsync(It.Is<User>(u =>
            u.Email == "ana@example.com" &&
            u.Name == "Ana" &&
            u.PasswordHash == "hashed-pwd")), Times.Once);
        refreshRepo.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
        ownerProvisioner.Verify(x => x.EnsureOwnerPersonAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldReturnTokens()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "ana@example.com",
            Name = "Ana",
            PasswordHash = "hash",
        };

        var userRepo = new Mock<IUserRepository>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var jwtGenerator = new Mock<IJwtGenerator>();
        var refreshRepo = new Mock<IRefreshTokenRepository>();
        var ownerProvisioner = new Mock<IOwnerPersonProvisioner>();

        userRepo.Setup(x => x.GetByEmailAsync("ana@example.com")).ReturnsAsync(user);
        passwordHasher.Setup(x => x.Verify("password-123", "hash")).Returns(true);
        jwtGenerator.Setup(x => x.Generate(user.Id, user.TenantId, user.Email, user.Name)).Returns("jwt-token");

        var useCase = new LoginUseCase(
            userRepo.Object,
            passwordHasher.Object,
            jwtGenerator.Object,
            refreshRepo.Object,
            ownerProvisioner.Object);

        var result = await useCase.ExecuteAsync(new LoginRequest("ana@example.com", "password-123"));

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("jwt-token");
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        refreshRepo.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
        ownerProvisioner.Verify(x => x.EnsureOwnerPersonAsync(user), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ShouldRotateAndReturnNewTokens()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "ana@example.com",
            Name = "Ana",
        };
        var oldRefresh = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = user.TenantId,
            Token = "old-refresh",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(10),
        };

        var refreshRepo = new Mock<IRefreshTokenRepository>();
        var userRepo = new Mock<IUserRepository>();
        var jwtGenerator = new Mock<IJwtGenerator>();

        refreshRepo.Setup(x => x.GetByTokenAsync("old-refresh")).ReturnsAsync(oldRefresh);
        userRepo.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        jwtGenerator.Setup(x => x.Generate(user.Id, user.TenantId, user.Email, user.Name)).Returns("jwt-token");

        var useCase = new RefreshTokenUseCase(refreshRepo.Object, userRepo.Object, jwtGenerator.Object);

        var result = await useCase.ExecuteAsync(new RefreshTokenRequest("old-refresh"));

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("jwt-token");
        result.RefreshToken.Should().NotBe("old-refresh");
        refreshRepo.Verify(x => x.RevokeAsync(oldRefresh, It.IsAny<DateTime>()), Times.Once);
        refreshRepo.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
    }
}
