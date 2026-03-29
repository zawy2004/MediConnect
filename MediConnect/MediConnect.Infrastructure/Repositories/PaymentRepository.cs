using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly MediconnectContext _context;

    public PaymentRepository(MediconnectContext context)
    {
        _context = context;
    }

    public Task<Payment> CreateAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        return Task.FromResult(payment);
    }

    public async Task<List<Payment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.CreatedAt >= fromDate && p.CreatedAt <= toDate)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetByPatientIdAsync(int patientId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Payment?> GetLatestByAppointmentAsync(int appointmentId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.AppointmentId == appointmentId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<decimal> SumPaidAmountAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.Payments
            .Where(p => p.CreatedAt >= fromDate && p.CreatedAt <= toDate && p.PaymentStatus == "PAID")
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;
    }

    public async Task<decimal> SumPaidAmountByDoctorAsync(int doctorId, DateTime fromDate, DateTime toDate)
    {
        return await _context.Payments
            .Where(p => p.CreatedAt >= fromDate
                        && p.CreatedAt <= toDate
                        && p.PaymentStatus == "PAID"
                        && p.Appointment != null
                        && p.Appointment.DoctorId == doctorId)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;
    }
}
