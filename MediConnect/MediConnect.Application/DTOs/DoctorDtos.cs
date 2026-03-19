namespace MediConnect.Application.DTOs;

public class DoctorListDto
{
    public int DoctorProfileId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string? Education { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal ConsultationFee { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public string? DepartmentName { get; set; }
    public string? Location { get; set; }
    public string? InsuranceAccepted { get; set; }
    public List<string> Specialties { get; set; } = new();
}

public class DoctorDetailDto
{
    public int DoctorProfileId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public string LicenseNumber { get; set; } = null!;
    public int YearsOfExperience { get; set; }
    public string? Education { get; set; }
    public string? Bio { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? InsuranceAccepted { get; set; }
    public string? Location { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public string? DepartmentName { get; set; }
    public List<string> Specialties { get; set; } = new();
    public List<ReviewDto> RecentReviews { get; set; } = new();
}

public class DoctorSearchFilterDto
{
    public string? SearchTerm { get; set; }
    public int? SpecialtyId { get; set; }
    public int? DepartmentId { get; set; }
}
