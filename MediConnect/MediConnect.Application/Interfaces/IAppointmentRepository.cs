using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface IAppointmentRepository
{
    Task<List<Appointment>> GetAllAsync();
    Task<List<Appointment>> GetByPatientIdAsync(int patientId);
    Task<List<Appointment>> GetByDoctorIdAsync(int doctorId);
    Task<Appointment?> GetByIdAsync(int appointmentId);
    Task<Appointment> CreateAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task<int> CountAsync();
}
