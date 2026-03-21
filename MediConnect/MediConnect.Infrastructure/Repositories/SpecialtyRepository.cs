using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class SpecialtyRepository : ISpecialtyRepository
{
    private readonly MediconnectContext _context;

    public SpecialtyRepository(MediconnectContext context)
    {
        _context = context;
    }

    public async Task<List<Specialty>> GetActiveAsync()
    {
        return await _context.Specialties
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.SpecialtyName)
            .ToListAsync();
    }

    public async Task<Specialty?> GetByIdAsync(int specialtyId)
    {
        return await _context.Specialties
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SpecialtyId == specialtyId);
    }

    public async Task<int> CountActiveAsync()
    {
        return await _context.Specialties.CountAsync(s => s.IsActive);
    }

    public Task UpdateAsync(Specialty specialty)
    {
        _context.Specialties.Update(specialty);
        return Task.CompletedTask;
    }
}
