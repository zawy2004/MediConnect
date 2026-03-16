namespace MediConnect.Application.DTOs;

public class DashboardDto
{
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalAppointments { get; set; }
    public int TotalSpecialties { get; set; }
    public List<SpecialtyDto> FeaturedSpecialties { get; set; } = new();
}
