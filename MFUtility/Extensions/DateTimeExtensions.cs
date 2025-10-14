using System;

namespace MFUtility.Extensions;

public static class DateTimeExtensions
{
    public static string ToReadableString(this TimeSpan ts)
    {
        if (ts.TotalDays >= 1) return $"{(int)ts.TotalDays}天前";
        if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours}小时前";
        if (ts.TotalMinutes >= 1) return $"{(int)ts.TotalMinutes}分钟前";
        return "刚刚";
    }

    public static string ToDateTimeString(this DateTime dt, bool includeSeconds = false)
        => includeSeconds ? dt.ToString("yyyy-MM-dd HH:mm:ss") : dt.ToString("yyyy-MM-dd HH:mm");

    public static bool IsToday(this DateTime date)
        => date.Date == DateTime.Today;

    public static DateTime StartOfWeek(this DateTime dt)
        => dt.AddDays(-(int)dt.DayOfWeek + (int)DayOfWeek.Monday);

    public static DateTime EndOfWeek(this DateTime dt)
        => dt.StartOfWeek().AddDays(6);
}
