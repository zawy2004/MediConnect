namespace MediConnect.Application.DTOs;

public class SpecialtyDto
{
    public int SpecialtyId { get; set; }
    public string SpecialtyName { get; set; } = null!;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; }
}
