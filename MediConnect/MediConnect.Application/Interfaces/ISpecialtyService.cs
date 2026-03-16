using MediConnect.Application.DTOs;

namespace MediConnect.Application.Interfaces;

public interface ISpecialtyService
{
    Task<List<SpecialtyDto>> GetActiveSpecialtiesAsync();
    Task<SpecialtyDto?> GetByIdAsync(int specialtyId);
}
