using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface ITimeSlotRepository
{
    Task<TimeSlot?> GetByIdAsync(int slotId);
    Task<List<TimeSlot>> GetAvailableAsync();
    Task<List<TimeSlot>> GetByDoctorAndDateRangeAsync(int doctorUserId, DateOnly fromDate, DateOnly toDate);
    Task UpdateAsync(TimeSlot slot);
}
