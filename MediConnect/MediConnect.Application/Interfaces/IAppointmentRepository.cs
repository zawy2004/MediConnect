using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface IAppointmentRepository
{
    Task<List<Appointment>> GetAllAsync();
    Task<List<Appointment>> GetByPatientIdAsync(int patientId);
    Task<List<Appointment>> GetByDoctorIdAsync(int doctorId);
    Task<List<Appointment>> GetUpcomingByPatientIdAsync(int patientId, int take);
    Task<List<Appointment>> GetUpcomingByDoctorIdAsync(int doctorId, int take);
    Task<List<Appointment>> GetByDateRangeAsync(DateOnly fromDate, DateOnly toDate);
    Task<List<Appointment>> GetRecentAsync(int take);
    Task<Appointment?> GetByIdAsync(int appointmentId);
    Task<Appointment> CreateAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task<int> CountAsync();
    Task<int> CountByPatientIdAsync(int patientId);
    Task<int> CountByDoctorIdAsync(int doctorId);
    Task<int> CountByStatusAsync(string status);
}
