using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class ComplaintRepository : IComplaintRepository
{
    private readonly MediconnectContext _context;

    public ComplaintRepository(MediconnectContext context)
    {
        _context = context;
    }

    public async Task<List<Complaint>> GetAllAsync()
    {
        return await _context.Complaints
            .AsNoTracking()
            .Include(c => c.Patient)
            .Include(c => c.Doctor)
            .Include(c => c.ResolvedByNavigation)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Complaint?> GetByIdAsync(int complaintId)
    {
        return await _context.Complaints
            .Include(c => c.Patient)
            .Include(c => c.Doctor)
            .Include(c => c.ResolvedByNavigation)
            .FirstOrDefaultAsync(c => c.ComplaintId == complaintId);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Complaints.CountAsync();
    }

    public async Task<int> CountByStatusAsync(string status)
    {
        return await _context.Complaints.CountAsync(c => c.Status == status);
    }

    public Task UpdateAsync(Complaint complaint)
    {
        _context.Complaints.Update(complaint);
        return Task.CompletedTask;
    }
}
