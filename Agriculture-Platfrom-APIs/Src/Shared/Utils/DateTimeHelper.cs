namespace AgriculturalMonitorSystem.Src.Shared.Utils;

public static class DateTimeHelper
{
    public static DateTime StartOfDay(DateTime date) => date.Date;
    public static DateTime EndOfDay(DateTime date) => date.Date.AddDays(1).AddTicks(-1);
    public static DateTime StartOfWeek(DateTime date) => date.AddDays(-(int)date.DayOfWeek).Date;
    public static DateTime StartOfMonth(DateTime date) => new(date.Year, date.Month, 1);

    public static string ToRelativeTime(DateTime date)
    {
        var diff = DateTime.UtcNow - date;
        return diff.TotalSeconds < 60 ? "just now"
            : diff.TotalMinutes < 60 ? $"{(int)diff.TotalMinutes}m ago"
            : diff.TotalHours < 24 ? $"{(int)diff.TotalHours}h ago"
            : diff.TotalDays < 7 ? $"{(int)diff.TotalDays}d ago"
            : date.ToString("MMM dd, yyyy");
    }
}
