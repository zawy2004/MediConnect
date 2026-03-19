namespace MediConnect.Application.DTOs;

public class TimeSlotDto
{
    public int SlotId { get; set; }
    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public string Display { get; set; } = null!;
}
