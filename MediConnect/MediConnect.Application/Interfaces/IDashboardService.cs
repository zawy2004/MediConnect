using MediConnect.Application.DTOs;

namespace MediConnect.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardDataAsync();
}
