using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;

namespace MediConnect.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IReviewRepository _reviewRepository;

    public DoctorService(
        IDoctorRepository doctorRepository,
        ISpecialtyRepository specialtyRepository,
        IDepartmentRepository departmentRepository,
        IReviewRepository reviewRepository)
    {
        _doctorRepository = doctorRepository;
        _specialtyRepository = specialtyRepository;
        _departmentRepository = departmentRepository;
        _reviewRepository = reviewRepository;
    }

    public async Task<List<DoctorListDto>> SearchDoctorsAsync(DoctorSearchFilterDto filter)
    {
        var doctors = await _doctorRepository.GetApprovedDoctorsAsync(
            filter.SearchTerm, filter.SpecialtyId, filter.DepartmentId);

        return doctors.Select(d => new DoctorListDto
        {
            DoctorProfileId = d.DoctorProfileId,
            UserId = d.UserId,
            FullName = d.User.FullName,
            AvatarUrl = d.User.AvatarUrl,
            Education = d.Education,
            YearsOfExperience = d.YearsOfExperience,
            ConsultationFee = d.ConsultationFee,
            AverageRating = d.AverageRating,
            TotalReviews = d.TotalReviews,
            DepartmentName = d.Department?.DepartmentName,
            Location = d.Location,
            InsuranceAccepted = d.InsuranceAccepted,
            Specialties = d.User.DoctorSpecialties
                .Select(ds => ds.Specialty.SpecialtyName)
                .ToList()
        }).ToList();
    }

    public async Task<DoctorDetailDto?> GetDoctorDetailAsync(int doctorProfileId)
    {
        var doctor = await _doctorRepository.GetDoctorDetailAsync(doctorProfileId);
        if (doctor == null) return null;

        var reviews = await _reviewRepository.GetByDoctorIdAsync(doctor.UserId, 10);

        return new DoctorDetailDto
        {
            DoctorProfileId = doctor.DoctorProfileId,
            UserId = doctor.UserId,
            FullName = doctor.User.FullName,
            AvatarUrl = doctor.User.AvatarUrl,
            PhoneNumber = doctor.User.PhoneNumber,
            Email = doctor.User.Email,
            Gender = doctor.User.Gender,
            LicenseNumber = doctor.LicenseNumber,
            YearsOfExperience = doctor.YearsOfExperience,
            Education = doctor.Education,
            Bio = doctor.Bio,
            ConsultationFee = doctor.ConsultationFee,
            InsuranceAccepted = doctor.InsuranceAccepted,
            Location = doctor.Location,
            AverageRating = doctor.AverageRating,
            TotalReviews = doctor.TotalReviews,
            DepartmentName = doctor.Department?.DepartmentName,
            Specialties = doctor.User.DoctorSpecialties
                .Select(ds => ds.Specialty.SpecialtyName)
                .ToList(),
            RecentReviews = reviews.Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                PatientName = r.Patient.FullName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList()
        };
    }

    public async Task<List<SpecialtyDto>> GetActiveSpecialtiesAsync()
    {
        var specialties = await _specialtyRepository.GetActiveAsync();
        return specialties.Select(s => new SpecialtyDto
        {
            SpecialtyId = s.SpecialtyId,
            SpecialtyName = s.SpecialtyName,
            Description = s.Description,
            IconUrl = s.IconUrl,
            IsActive = s.IsActive
        }).ToList();
    }

    public async Task<List<DepartmentDto>> GetActiveDepartmentsAsync()
    {
        var departments = await _departmentRepository.GetActiveAsync();
        return departments.Select(d => new DepartmentDto
        {
            DepartmentId = d.DepartmentId,
            DepartmentName = d.DepartmentName,
            Description = d.Description,
            Location = d.Location,
            IsActive = d.IsActive
        }).ToList();
    }
}
