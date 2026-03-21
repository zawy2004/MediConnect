using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;

namespace MediConnect.Application.Services;

public class PortalService : IPortalService
{
    private readonly IUserRepository _userRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IDoctorService _doctorService;
    private readonly IUnitOfWork _unitOfWork;

    public PortalService(
        IUserRepository userRepository,
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IDoctorService doctorService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _doctorService = doctorService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PatientDashboardDto> GetPatientDashboardAsync(int patientId)
    {
        var user = await _userRepository.GetByIdAsync(patientId);
        var totalAppointments = await _appointmentRepository.CountByPatientIdAsync(patientId);
        var upcoming = await _appointmentRepository.GetUpcomingByPatientIdAsync(patientId, 6);
        var recommended = await _doctorService.SearchDoctorsAsync(new DoctorSearchFilterDto());

        return new PatientDashboardDto
        {
            PatientName = user?.FullName ?? "Bệnh nhân",
            TotalAppointments = totalAppointments,
            UpcomingAppointments = upcoming.Count,
            UpcomingList = upcoming.Select(MapToAppointmentList).ToList(),
            RecommendedDoctors = recommended.Take(4).ToList()
        };
    }

    public async Task<DoctorDashboardDto> GetDoctorDashboardAsync(int doctorUserId)
    {
        var user = await _userRepository.GetByIdAsync(doctorUserId);
        var profile = await _doctorRepository.GetByUserIdAsync(doctorUserId);
        var totalAppointments = await _appointmentRepository.CountByDoctorIdAsync(doctorUserId);
        var upcoming = await _appointmentRepository.GetUpcomingByDoctorIdAsync(doctorUserId, 8);

        var pendingAppointments = upcoming.Count(a => a.Status == AppointmentStatus.Pending);
        var confirmedAppointments = upcoming.Count(a => a.Status == AppointmentStatus.Confirmed);

        DoctorDetailDto? detail = null;
        if (profile != null)
        {
            detail = await _doctorService.GetDoctorDetailAsync(profile.DoctorProfileId);
        }

        return new DoctorDashboardDto
        {
            DoctorName = user?.FullName ?? "Bác sĩ",
            DepartmentName = profile?.Department?.DepartmentName,
            TotalAppointments = totalAppointments,
            PendingAppointments = pendingAppointments,
            ConfirmedAppointments = confirmedAppointments,
            UpcomingList = upcoming.Select(MapToAppointmentList).ToList(),
            Profile = detail
        };
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        var totalDoctors = await _userRepository.CountByRoleAsync(RoleNames.Doctor);
        var totalPatients = await _userRepository.CountByRoleAsync(RoleNames.Patient);
        var totalAppointments = await _appointmentRepository.CountAsync();
        var pendingDoctors = await _doctorRepository.GetPendingApprovalAsync(8);
        var recentUsers = await _userRepository.GetRecentUsersAsync(10);
        var pendingAppointments = await _appointmentRepository.CountByStatusAsync(AppointmentStatus.Pending);

        return new AdminDashboardDto
        {
            TotalUsers = totalDoctors + totalPatients,
            TotalDoctors = totalDoctors,
            TotalPatients = totalPatients,
            TotalAppointments = totalAppointments,
            PendingDoctorApprovals = pendingDoctors.Count,
            PendingAppointments = pendingAppointments,
            PendingDoctors = pendingDoctors.Select(d => new DoctorApprovalItemDto
            {
                DoctorProfileId = d.DoctorProfileId,
                UserId = d.UserId,
                FullName = d.User.FullName,
                DepartmentName = d.Department?.DepartmentName,
                Email = d.User.Email,
                YearsOfExperience = d.YearsOfExperience,
                CreatedAt = d.CreatedAt
            }).ToList(),
            RecentUsers = recentUsers.Select(u => new SystemUserItemDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                RoleName = u.Role.RoleName,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList()
        };
    }

    public async Task<bool> UpdateDoctorProfileAsync(int doctorUserId, string bio, decimal consultationFee, string? insuranceAccepted, string? location)
    {
        var profile = await _doctorRepository.GetByUserIdAsync(doctorUserId);
        if (profile == null)
        {
            return false;
        }

        profile.Bio = bio;
        profile.ConsultationFee = consultationFee;
        profile.InsuranceAccepted = insuranceAccepted;
        profile.Location = location;
        profile.UpdatedAt = DateTime.Now;

        await _doctorRepository.UpdateAsync(profile);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static AppointmentListDto MapToAppointmentList(Domain.Entities.Appointment appointment)
    {
        return new AppointmentListDto
        {
            AppointmentId = appointment.AppointmentId,
            PatientName = appointment.Patient.FullName,
            DoctorName = appointment.Doctor.FullName,
            SpecialtyName = appointment.Specialty?.SpecialtyName,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Reason = appointment.Reason,
            Status = appointment.Status,
            CreatedAt = appointment.CreatedAt
        };
    }
}
