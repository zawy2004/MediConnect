using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Departments;

public class IndexModel : PageModel
{
    private readonly IDepartmentService _departmentService;

    public IndexModel(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    public List<DepartmentDto> Departments { get; set; } = new();

    public async Task OnGetAsync()
    {
        Departments = await _departmentService.GetActiveDepartmentsAsync();
    }
}
