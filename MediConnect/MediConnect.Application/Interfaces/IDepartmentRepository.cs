using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();
    Task<List<Department>> GetActiveAsync();
    Task<Department?> GetByIdAsync(int departmentId);
    Task CreateAsync(Department department);
    Task UpdateAsync(Department department);
}
