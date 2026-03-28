using MediConnect.Domain.Entities;

namespace MediConnect.Application.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetRecentByChannelAsync(string channel, int take);
    Task<List<Notification>> GetRecentByUserIdAndChannelAsync(int userId, string channel, int take);
    Task<int> CountUnreadInAppByUserIdAsync(int userId);
    Task MarkAllInAppAsReadForUserAsync(int userId);
    Task<int> CountByChannelAsync(string channel);
    Task<int> CountByChannelAndStatusAsync(string channel, string status);
    Task<int> CountReadByChannelAsync(string channel);
    Task CreateAsync(Notification notification);
}
