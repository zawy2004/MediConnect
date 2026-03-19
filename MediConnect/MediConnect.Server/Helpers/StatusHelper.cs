namespace MediConnect.Server.Helpers;

public static class StatusHelper
{
    public static string GetBadgeClass(string status)
    {
        if (status == "PENDING") return "badge-pending";
        if (status == "CONFIRMED") return "badge-confirmed";
        if (status == "COMPLETED") return "badge-completed";
        if (status.StartsWith("CANCELLED")) return "badge-cancelled";
        return "bg-secondary";
    }
}
