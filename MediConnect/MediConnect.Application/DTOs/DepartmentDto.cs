namespace MediConnect.Application.DTOs;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = null!;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
}
