using System;
using System.Collections.Generic;

namespace MediConnect.Domain.Entities;

public partial class Review
{
    public int ReviewId { get; set; }

    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public byte Rating { get; set; }

    public string? Comment { get; set; }

    public decimal? SentimentScore { get; set; }

    public string? SentimentLabel { get; set; }

    public bool IsVisible { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual User Doctor { get; set; } = null!;

    public virtual User Patient { get; set; } = null!;
}
