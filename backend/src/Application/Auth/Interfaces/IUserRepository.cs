using Domain.Entities;

namespace Application.Auth.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByGoogleIdAsync(string googleId);
    Task CreateAsync(User user);
}
