using Application.Auth.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IMongoCollection<RefreshToken> _refreshTokens;

    public RefreshTokenRepository(AppDbContext context)
    {
        _refreshTokens = context.RefreshTokens;
    }

    public async Task CreateAsync(RefreshToken refreshToken)
        => await _refreshTokens.InsertOneAsync(refreshToken);

    public async Task<RefreshToken?> GetByTokenAsync(string token)
        => await _refreshTokens.Find(x => x.Token == token).FirstOrDefaultAsync();

    public async Task RevokeAsync(RefreshToken refreshToken, DateTime revokedAtUtc)
    {
        refreshToken.RevokedAtUtc = revokedAtUtc;
        await _refreshTokens.ReplaceOneAsync(x => x.Id == refreshToken.Id, refreshToken);
    }
}
