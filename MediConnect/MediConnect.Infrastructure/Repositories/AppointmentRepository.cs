using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly MediconnectContext _context;

    public AppointmentRepository(MediconnectContext context)
    {
        _context = context;
    }

    public async Task<List<Appointment>> GetAllAsync()
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Specialty)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetByPatientIdAsync(int patientId)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Specialty)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Specialty)
            .Where(a => a.DoctorId == doctorId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetUpcomingByPatientIdAsync(int patientId, int take)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Specialty)
            .Where(a => a.PatientId == patientId && a.AppointmentDate >= today)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetUpcomingByDoctorIdAsync(int doctorId, int take)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Specialty)
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate >= today)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetByDateRangeAsync(DateOnly fromDate, DateOnly toDate)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Specialty)
            .Where(a => a.AppointmentDate >= fromDate && a.AppointmentDate <= toDate)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetRecentAsync(int take)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Specialty)
            .OrderByDescending(a => a.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<Appointment?> GetByIdAsync(int appointmentId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Specialty)
            .Include(a => a.Slot)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
    }

    public Task<Appointment> CreateAsync(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        return Task.FromResult(appointment);
    }

    public Task UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        return Task.CompletedTask;
    }

    public async Task<int> CountAsync()
    {
        return await _context.Appointments.CountAsync();
    }

    public async Task<int> CountByPatientIdAsync(int patientId)
    {
        return await _context.Appointments.CountAsync(a => a.PatientId == patientId);
    }

    public async Task<int> CountByDoctorIdAsync(int doctorId)
    {
        return await _context.Appointments.CountAsync(a => a.DoctorId == doctorId);
    }

    public async Task<int> CountByStatusAsync(string status)
    {
        return await _context.Appointments.CountAsync(a => a.Status == status);
    }
}
