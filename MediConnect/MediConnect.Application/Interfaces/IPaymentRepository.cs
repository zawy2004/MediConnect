using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Payment> CreateAsync(Payment payment);
    Task<List<Payment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<List<Payment>> GetByPatientIdAsync(int patientId);
    Task<Payment?> GetLatestByAppointmentAsync(int appointmentId);
    Task<decimal> SumPaidAmountAsync(DateTime fromDate, DateTime toDate);
    Task<decimal> SumPaidAmountByDoctorAsync(int doctorId, DateTime fromDate, DateTime toDate);
}
