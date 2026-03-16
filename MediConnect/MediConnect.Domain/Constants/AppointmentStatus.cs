namespace MediConnect.Domain.Constants;

public static class AppointmentStatus
{
    public const string Pending = "PENDING";
    public const string Confirmed = "CONFIRMED";
    public const string Completed = "COMPLETED";
    public const string CancelledByPatient = "CANCELLED_BY_PATIENT";
    public const string CancelledByDoctor = "CANCELLED_BY_DOCTOR";
    public const string NoShow = "NO_SHOW";

    public static bool IsCancellable(string status)
        => status is Pending or Confirmed;
}
