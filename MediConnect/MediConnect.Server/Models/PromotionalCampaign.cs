using System;
using System.Collections.Generic;

namespace MediConnect.Server.Models;

public partial class PromotionalCampaign
{
    public int CampaignId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string TargetRole { get; set; } = null!;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int CreatedBy { get; set; }

    public bool IsSent { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;
}
