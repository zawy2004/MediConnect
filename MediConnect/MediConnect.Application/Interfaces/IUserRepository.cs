using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int userId);
    Task<bool> EmailExistsAsync(string email);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<int> CountByRoleAsync(string roleName);
}
