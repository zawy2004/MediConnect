using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly MediconnectContext _context;

    public RoleRepository(MediconnectContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == roleName);
    }
}
