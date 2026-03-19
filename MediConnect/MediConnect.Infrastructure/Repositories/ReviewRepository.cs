using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly MediconnectContext _context;

    public ReviewRepository(MediconnectContext context)
    {
        _context = context;
    }

    public async Task<List<Review>> GetByDoctorIdAsync(int doctorId, int take = 10)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Include(r => r.Patient)
            .Where(r => r.DoctorId == doctorId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(take)
            .ToListAsync();
    }
}
