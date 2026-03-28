using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface ISystemLogRepository
{
    Task<List<SystemLog>> GetRecentAsync(int take, string? severity = null);
    Task<List<SystemLog>> GetByUserIdAsync(int userId, int take = 50);
    Task<int> CountBySeverityAsync(string severity);
    Task<SystemLog> CreateAsync(SystemLog log);
    Task UpdateAsync(SystemLog log);
}
