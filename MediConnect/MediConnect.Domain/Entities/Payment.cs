using System;
using System.Collections.Generic;

namespace MediConnect.Domain.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? AppointmentId { get; set; }

    public int PatientId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public string? TransactionId { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? RefundedAt { get; set; }

    public decimal? RefundAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual User Patient { get; set; } = null!;
}
