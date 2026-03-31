using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface ISpecialtyRepository
{
    Task<List<Specialty>> GetAllAsync();
    Task<List<Specialty>> GetActiveAsync();
    Task<Specialty?> GetByIdAsync(int specialtyId);
    Task<int> CountActiveAsync();
    Task CreateAsync(Specialty specialty);
    Task UpdateAsync(Specialty specialty);
}
