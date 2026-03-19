using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;

namespace MediConnect.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUserRepository _userRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ISpecialtyRepository _specialtyRepository;

    public DashboardService(
        IUserRepository userRepository,
        IAppointmentRepository appointmentRepository,
        ISpecialtyRepository specialtyRepository)
    {
        _userRepository = userRepository;
        _appointmentRepository = appointmentRepository;
        _specialtyRepository = specialtyRepository;
    }

    public async Task<DashboardDto> GetDashboardDataAsync()
    {
        var totalPatients = await _userRepository.CountByRoleAsync(RoleNames.Patient);
        var totalDoctors = await _userRepository.CountByRoleAsync(RoleNames.Doctor);
        var totalAppointments = await _appointmentRepository.CountAsync();
        var totalSpecialties = await _specialtyRepository.CountActiveAsync();

        var specialties = await _specialtyRepository.GetActiveAsync();
        var featuredSpecialties = specialties
            .Take(8)
            .Select(s => new SpecialtyDto
            {
                SpecialtyId = s.SpecialtyId,
                SpecialtyName = s.SpecialtyName,
                Description = s.Description,
                IconUrl = s.IconUrl,
                IsActive = s.IsActive
            })
            .ToList();

        return new DashboardDto
        {
            TotalPatients = totalPatients,
            TotalDoctors = totalDoctors,
            TotalAppointments = totalAppointments,
            TotalSpecialties = totalSpecialties,
            FeaturedSpecialties = featuredSpecialties
        };
    }
}
