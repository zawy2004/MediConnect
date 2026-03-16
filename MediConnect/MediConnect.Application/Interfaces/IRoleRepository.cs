using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string roleName);
}
