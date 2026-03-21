using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface ISystemLogRepository
{
    Task<List<SystemLog>> GetRecentAsync(int take, string? severity = null);
    Task<int> CountBySeverityAsync(string severity);
}
