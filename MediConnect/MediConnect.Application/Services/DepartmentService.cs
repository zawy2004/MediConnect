using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;

namespace MediConnect.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<List<DepartmentDto>> GetActiveDepartmentsAsync()
    {
        var departments = await _departmentRepository.GetActiveAsync();
        return departments.Select(d => new DepartmentDto
        {
            DepartmentId = d.DepartmentId,
            DepartmentName = d.DepartmentName,
            Description = d.Description,
            Location = d.Location,
            IsActive = d.IsActive
        }).ToList();
    }

    public async Task<DepartmentDto?> GetByIdAsync(int departmentId)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId);
        if (department == null) return null;

        return new DepartmentDto
        {
            DepartmentId = department.DepartmentId,
            DepartmentName = department.DepartmentName,
            Description = department.Description,
            Location = department.Location,
            IsActive = department.IsActive
        };
    }
}
