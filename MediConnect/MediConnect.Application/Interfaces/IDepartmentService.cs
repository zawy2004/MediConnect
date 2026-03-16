using MediConnect.Application.DTOs;

namespace MediConnect.Application.Interfaces;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetActiveDepartmentsAsync();
    Task<DepartmentDto?> GetByIdAsync(int departmentId);
}
