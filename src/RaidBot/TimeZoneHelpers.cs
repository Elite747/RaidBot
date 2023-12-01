namespace RaidBot;

public static class TimeZoneHelpers
{
    public static DateTimeOffset ConvertTimeToLocal(DateTime dateTime, string timezone, bool isUtc)
    {
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(timezone, out var tz))
        {
            tz = TimeZoneInfo.Utc;
        }

        if (isUtc)
        {
            return TimeZoneInfo.ConvertTime(new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, TimeSpan.Zero), tz);
        }

        return new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, tz.GetUtcOffset(dateTime));
    }
}
