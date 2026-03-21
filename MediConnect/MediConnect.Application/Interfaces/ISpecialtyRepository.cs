using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface ISpecialtyRepository
{
    Task<List<Specialty>> GetActiveAsync();
    Task<Specialty?> GetByIdAsync(int specialtyId);
    Task<int> CountActiveAsync();
    Task UpdateAsync(Specialty specialty);
}
