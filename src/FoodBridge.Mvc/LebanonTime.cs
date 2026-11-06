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
}
