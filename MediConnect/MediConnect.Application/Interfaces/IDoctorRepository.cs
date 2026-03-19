using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface IDoctorRepository
{
    Task<List<DoctorProfile>> GetApprovedDoctorsAsync(string? searchTerm, int? specialtyId, int? departmentId);
    Task<DoctorProfile?> GetDoctorDetailAsync(int doctorProfileId);
}
