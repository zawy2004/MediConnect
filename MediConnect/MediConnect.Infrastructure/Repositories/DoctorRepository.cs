using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly MediconnectContext _context;

    public DoctorRepository(MediconnectContext context)
    {
        _context = context;
    }

    public async Task<List<DoctorProfile>> GetApprovedDoctorsAsync(string? searchTerm, int? specialtyId, int? departmentId)
    {
        var query = _context.DoctorProfiles
            .AsNoTracking()
            .Include(d => d.User)
                .ThenInclude(u => u.DoctorSpecialties)
                    .ThenInclude(ds => ds.Specialty)
            .Include(d => d.Department)
            .Where(d => d.ApprovalStatus == DoctorApprovalStatus.Approved && d.User.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(d => d.User.FullName.Contains(searchTerm));
        }

        if (specialtyId.HasValue)
        {
            query = query.Where(d => d.User.DoctorSpecialties.Any(ds => ds.SpecialtyId == specialtyId.Value));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(d => d.DepartmentId == departmentId.Value);
        }

        return await query.OrderBy(d => d.User.FullName).ToListAsync();
    }

    public async Task<DoctorProfile?> GetDoctorDetailAsync(int doctorProfileId)
    {
        return await _context.DoctorProfiles
            .AsNoTracking()
            .Include(d => d.User)
                .ThenInclude(u => u.DoctorSpecialties)
                    .ThenInclude(ds => ds.Specialty)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.DoctorProfileId == doctorProfileId);
    }
}
