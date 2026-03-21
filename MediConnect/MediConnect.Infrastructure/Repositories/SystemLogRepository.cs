using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class SystemLogRepository : ISystemLogRepository
{
    private readonly MediconnectContext _context;

    public SystemLogRepository(MediconnectContext context)
    {
        _context = context;
    }

    public async Task<List<SystemLog>> GetRecentAsync(int take, string? severity = null)
    {
        var query = _context.SystemLogs
            .AsNoTracking()
            .Include(l => l.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(severity))
        {
            query = query.Where(l => l.Severity == severity);
        }

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> CountBySeverityAsync(string severity)
    {
        return await _context.SystemLogs.CountAsync(l => l.Severity == severity);
    }
}
