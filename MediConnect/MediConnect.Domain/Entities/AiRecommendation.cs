using System;
using System.Collections.Generic;

namespace MediConnect.Domain.Entities;

public partial class AiRecommendation
{
    public int RecommendationId { get; set; }

    public int PatientId { get; set; }

    public string InputSymptoms { get; set; } = null!;

    public string? RecommendedDoctors { get; set; }

    public string? RecommendedSlots { get; set; }

    public int? SpecialtySuggested { get; set; }

    public string? ModelVersion { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User Patient { get; set; } = null!;

    public virtual Specialty? SpecialtySuggestedNavigation { get; set; }
}
