using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class DoctorSpecialtyRepository : IDoctorSpecialtyRepository
{
    private readonly MediconnectContext _context;

    public DoctorSpecialtyRepository(MediconnectContext context)
    {
        _context = context;
    }

    public async Task<List<DoctorSpecialty>> GetBySpecialtyIdAsync(int specialtyId)
    {
        return await _context.DoctorSpecialties
            .AsNoTracking()
            .Include(ds => ds.User)
            .Where(ds => ds.SpecialtyId == specialtyId)
            .ToListAsync();
    }

    public async Task RemoveBySpecialtyIdAsync(int specialtyId)
    {
        var rows = await _context.DoctorSpecialties.Where(ds => ds.SpecialtyId == specialtyId).ToListAsync();
        _context.DoctorSpecialties.RemoveRange(rows);
    }

    public Task AddRangeAsync(List<DoctorSpecialty> mappings)
    {
        _context.DoctorSpecialties.AddRange(mappings);
        return Task.CompletedTask;
    }
}
