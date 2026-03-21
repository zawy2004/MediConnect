using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MediconnectContext _context;

    public UserRepository(MediconnectContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetRecentUsersAsync(int take)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .OrderByDescending(u => u.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<User>> SearchUsersAsync(string? searchTerm, string? roleName, int take = 200)
    {
        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u =>
                u.FullName.Contains(searchTerm) ||
                u.Email.Contains(searchTerm) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(roleName))
        {
            query = query.Where(u => u.Role.RoleName == roleName);
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> CountAllActiveAsync()
    {
        return await _context.Users.CountAsync(u => u.IsActive);
    }

    public Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task<int> CountByRoleAsync(string roleName)
    {
        return await _context.Users
            .CountAsync(u => u.Role.RoleName == roleName && u.IsActive);
    }
}
