using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface IReviewRepository
{
    Task<List<Review>> GetByDoctorIdAsync(int doctorId, int take = 10);
}
