using Domain.Entities;

namespace Application.Auth.Interfaces;

public interface IOwnerPersonProvisioner
{
    Task EnsureOwnerPersonAsync(User user);
}
