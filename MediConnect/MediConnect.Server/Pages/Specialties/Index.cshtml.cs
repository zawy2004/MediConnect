using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Specialties;

public class IndexModel : PageModel
{
    private readonly ISpecialtyService _specialtyService;

    public IndexModel(ISpecialtyService specialtyService)
    {
        _specialtyService = specialtyService;
    }

    public List<SpecialtyDto> Specialties { get; set; } = new();

    public async Task OnGetAsync()
    {
        Specialties = await _specialtyService.GetActiveSpecialtiesAsync();
    }
}
