using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;
using MediConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly MediconnectContext _context;

    public NotificationRepository(MediconnectContext context)
    {
        _context = context;
    }

    public async Task<List<Notification>> GetRecentByChannelAsync(string channel, int take)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Include(n => n.User)
            .Where(n => n.Channel == channel)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetRecentByUserIdAndChannelAsync(int userId, string channel, int take)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Include(n => n.User)
            .Where(n => n.UserId == userId && n.Channel == channel)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public Task<int> CountUnreadInAppByUserIdAsync(int userId)
    {
        return _context.Notifications.AsNoTracking()
            .CountAsync(n => n.UserId == userId && n.Channel == "IN_APP" && !n.IsRead);
    }

    public Task MarkAllInAppAsReadForUserAsync(int userId)
    {
        var now = DateTime.Now;
        return _context.Notifications
            .Where(n => n.UserId == userId && n.Channel == "IN_APP" && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now));
    }

    public async Task<int> CountByChannelAsync(string channel)
    {
        return await _context.Notifications.CountAsync(n => n.Channel == channel);
    }

    public async Task<int> CountByChannelAndStatusAsync(string channel, string status)
    {
        return await _context.Notifications.CountAsync(n => n.Channel == channel && n.Status == status);
    }

    public async Task<int> CountReadByChannelAsync(string channel)
    {
        return await _context.Notifications.CountAsync(n => n.Channel == channel && n.IsRead);
    }

    public Task CreateAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        return Task.CompletedTask;
    }
}
