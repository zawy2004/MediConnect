using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface IAppointmentWaitlistRepository
{
    Task<List<AppointmentWaitlist>> GetByDoctorIdAsync(int doctorId);
    Task<AppointmentWaitlist?> GetByIdAsync(int waitlistId);
    Task UpdateAsync(AppointmentWaitlist waitlist);
}
