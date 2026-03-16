using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface IDepartmentRepository
{
    Task<List<Department>> GetActiveAsync();
    Task<Department?> GetByIdAsync(int departmentId);
}
