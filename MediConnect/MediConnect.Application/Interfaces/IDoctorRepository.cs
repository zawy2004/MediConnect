using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface IDoctorRepository
{
    Task<List<DoctorProfile>> GetApprovedDoctorsAsync(string? searchTerm, int? specialtyId, int? departmentId);
    Task<DoctorProfile?> GetDoctorDetailAsync(int doctorProfileId);
    Task<DoctorProfile?> GetByUserIdAsync(int userId);
    Task<List<DoctorProfile>> GetPendingApprovalAsync(int take);
    Task<int> CountPendingApprovalAsync();
    Task UpdateAsync(DoctorProfile profile);
}
