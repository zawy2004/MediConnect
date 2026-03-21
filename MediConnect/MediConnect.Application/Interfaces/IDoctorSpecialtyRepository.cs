using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface IDoctorSpecialtyRepository
{
    Task<List<DoctorSpecialty>> GetBySpecialtyIdAsync(int specialtyId);
    Task RemoveBySpecialtyIdAsync(int specialtyId);
    Task AddRangeAsync(List<DoctorSpecialty> mappings);
}
