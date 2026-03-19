using MediConnect.Application.DTOs;

namespace MediConnect.Application.Interfaces;

public interface IDoctorService
{
    Task<List<DoctorListDto>> SearchDoctorsAsync(DoctorSearchFilterDto filter);
    Task<DoctorDetailDto?> GetDoctorDetailAsync(int doctorProfileId);
    Task<List<SpecialtyDto>> GetActiveSpecialtiesAsync();
    Task<List<DepartmentDto>> GetActiveDepartmentsAsync();
}
