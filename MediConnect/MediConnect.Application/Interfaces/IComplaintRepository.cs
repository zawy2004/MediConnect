using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface IComplaintRepository
{
    Task<List<Complaint>> GetAllAsync();
    Task<Complaint?> GetByIdAsync(int complaintId);
    Task<int> CountAsync();
    Task<int> CountByStatusAsync(string status);
    Task UpdateAsync(Complaint complaint);
}
