using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;

namespace MediConnect.Application.Services;

public class SpecialtyService : ISpecialtyService
{
    private readonly ISpecialtyRepository _specialtyRepository;

    public SpecialtyService(ISpecialtyRepository specialtyRepository)
    {
        _specialtyRepository = specialtyRepository;
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

    public async Task<SpecialtyDto?> GetByIdAsync(int specialtyId)
    {
        var specialty = await _specialtyRepository.GetByIdAsync(specialtyId);
        if (specialty == null) return null;

        return new SpecialtyDto
        {
            SpecialtyId = specialty.SpecialtyId,
            SpecialtyName = specialty.SpecialtyName,
            Description = specialty.Description,
            IconUrl = specialty.IconUrl,
            IsActive = specialty.IsActive
        };
    }
}
