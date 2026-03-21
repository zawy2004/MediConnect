using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly MediconnectContext _context;

    public MedicalRecordRepository(MediconnectContext context)
    {
        _context = context;
    }

    public async Task<List<MedicalRecord>> GetByDoctorAndPatientAsync(int doctorId, int patientId)
    {
        return await _context.MedicalRecords
            .Include(r => r.Doctor)
            .Include(r => r.Patient)
            .Where(r => r.DoctorId == doctorId && r.PatientId == patientId)
            .OrderByDescending(r => r.RecordDate)
            .ToListAsync();
    }

    public async Task<MedicalRecord?> GetLatestByDoctorAndPatientAsync(int doctorId, int patientId)
    {
        return await _context.MedicalRecords
            .Include(r => r.Doctor)
            .Include(r => r.Patient)
            .Where(r => r.DoctorId == doctorId && r.PatientId == patientId)
            .OrderByDescending(r => r.RecordDate)
            .ThenByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public Task CreateAsync(MedicalRecord record)
    {
        _context.MedicalRecords.Add(record);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(MedicalRecord record)
    {
        _context.MedicalRecords.Update(record);
        return Task.CompletedTask;
    }
}
