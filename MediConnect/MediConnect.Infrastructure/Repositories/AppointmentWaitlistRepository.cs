using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class AppointmentWaitlistRepository : IAppointmentWaitlistRepository
{
    private readonly MediconnectContext _context;

    public AppointmentWaitlistRepository(MediconnectContext context)
    {
        _context = context;
    }

    public async Task<List<AppointmentWaitlist>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.AppointmentWaitlists
            .Include(w => w.Patient)
            .Where(w => w.DoctorId == doctorId)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<AppointmentWaitlist?> GetByIdAsync(int waitlistId)
    {
        return await _context.AppointmentWaitlists
            .Include(w => w.Patient)
            .FirstOrDefaultAsync(w => w.WaitlistId == waitlistId);
    }

    public Task UpdateAsync(AppointmentWaitlist waitlist)
    {
        _context.AppointmentWaitlists.Update(waitlist);
        return Task.CompletedTask;
    }
}
