using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly MediconnectContext _context;

    public DepartmentRepository(MediconnectContext context)
    {
        _context = context;
    }

    public async Task<List<Department>> GetActiveAsync()
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int departmentId)
    {
        return await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
    }
}
