using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class TimeSlotRepository : ITimeSlotRepository
{
    private readonly MediconnectContext _context;

    public TimeSlotRepository(MediconnectContext context)
    {
        _context = context;
    }

    public async Task<TimeSlot?> GetByIdAsync(int slotId)
    {
        return await _context.TimeSlots
            .Include(s => s.Schedule)
            .FirstOrDefaultAsync(s => s.SlotId == slotId);
    }

    public async Task<List<TimeSlot>> GetAvailableAsync()
    {
        return await _context.TimeSlots
            .AsNoTracking()
            .Where(s => s.IsAvailable && s.BookedCount < s.MaxCapacity)
            .OrderBy(s => s.SlotDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<List<TimeSlot>> GetByDoctorAndDateRangeAsync(int doctorUserId, DateOnly fromDate, DateOnly toDate)
    {
        return await _context.TimeSlots
            .AsNoTracking()
            .Where(s => s.UserId == doctorUserId && s.SlotDate >= fromDate && s.SlotDate <= toDate)
            .OrderBy(s => s.SlotDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public Task UpdateAsync(TimeSlot slot)
    {
        _context.TimeSlots.Update(slot);
        return Task.CompletedTask;
    }
}
