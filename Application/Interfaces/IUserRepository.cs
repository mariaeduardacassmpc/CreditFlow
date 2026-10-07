using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRepository
{
    Task<User> CreateAsync(User user);

    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByIdAsync(int id);
}