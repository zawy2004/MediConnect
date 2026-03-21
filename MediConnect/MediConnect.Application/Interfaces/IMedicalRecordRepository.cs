using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface IMedicalRecordRepository
{
    Task<List<MedicalRecord>> GetByDoctorAndPatientAsync(int doctorId, int patientId);
    Task<MedicalRecord?> GetLatestByDoctorAndPatientAsync(int doctorId, int patientId);
    Task CreateAsync(MedicalRecord record);
    Task UpdateAsync(MedicalRecord record);
}
