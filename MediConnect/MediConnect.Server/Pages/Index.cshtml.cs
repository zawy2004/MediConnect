using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages;

public class IndexModel : PageModel
{
    private readonly IDashboardService _dashboardService;

    public IndexModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalAppointments { get; set; }
    public int TotalSpecialties { get; set; }
    public List<SpecialtyDto> Specialties { get; set; } = new();

    public async Task OnGetAsync()
    {
        var data = await _dashboardService.GetDashboardDataAsync();
        TotalPatients = data.TotalPatients;
        TotalDoctors = data.TotalDoctors;
        TotalAppointments = data.TotalAppointments;
        TotalSpecialties = data.TotalSpecialties;
        Specialties = data.FeaturedSpecialties;
    }
}
