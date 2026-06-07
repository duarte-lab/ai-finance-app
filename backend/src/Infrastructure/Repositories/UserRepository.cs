using Application.Auth.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(AppDbContext context)
    {
        _users = context.Users;
    }

    public async Task<User?> GetByIdAsync(Guid id)
        => await _users.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<User?> GetByGoogleIdAsync(string googleId)
        => await _users.Find(x => x.GoogleId == googleId).FirstOrDefaultAsync();

    public async Task<User?> GetByEmailAsync(string email)
        => await _users.Find(x => x.Email == email).FirstOrDefaultAsync();

    public async Task CreateAsync(User user)
        => await _users.InsertOneAsync(user);

    public async Task UpdateAsync(User user)
        => await _users.ReplaceOneAsync(x => x.Id == user.Id, user);
}
