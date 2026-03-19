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
}
