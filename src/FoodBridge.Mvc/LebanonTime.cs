namespace FoodBridge.Mvc;

public static class LebanonTime
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById(
        TimeZoneInfo.TryFindSystemTimeZoneById("Asia/Beirut", out var beirut)
            ? beirut.Id
            : "Middle East Standard Time");

    public static string Format(DateTime utc)
    {
        var value = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(value, Zone).ToString("dd MMM yyyy, HH:mm");
    }

    public static DateTime Now()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);
    }

    public static DateTime ToUtc(DateTime local)
    {
        var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, Zone);
    }
}
