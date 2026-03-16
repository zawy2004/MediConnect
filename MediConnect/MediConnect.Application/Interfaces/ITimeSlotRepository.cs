using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface ITimeSlotRepository
{
    Task<TimeSlot?> GetByIdAsync(int slotId);
    Task<List<TimeSlot>> GetAvailableAsync();
    Task UpdateAsync(TimeSlot slot);
}
