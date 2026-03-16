using System;
using System.Collections.Generic;

namespace MediConnect.Domain.Entities;

public partial class ExternalCalendarIntegration
{
    public int IntegrationId { get; set; }

    public int UserId { get; set; }

    public string Provider { get; set; } = null!;

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? TokenExpiresAt { get; set; }

    public string? CalendarId { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
