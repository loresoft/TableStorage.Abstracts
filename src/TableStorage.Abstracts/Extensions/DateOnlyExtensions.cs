namespace TableStorage.Abstracts.Extensions;

#if NET6_0_OR_GREATER
/// <summary>
/// Provides extension methods for <see cref="DateOnly" /> to support time zone conversions.
/// </summary>
public static class DateOnlyExtensions
{
    /// <summary>
    /// Converts the <see cref="DateOnly"/> to a <see cref="DateTimeOffset"/> in the specified time zone.
    /// </summary>
    /// <param name="dateOnly">The date to convert.</param>
    /// <param name="zone">The time zone to use for the conversion. Uses local time zone if not specified.</param>
    /// <returns>A <see cref="DateTimeOffset"/> representing the start of the specified date in the given time zone.</returns>
    /// <remarks>
    /// <para>
    /// This method creates a <see cref="DateTimeOffset"/> by combining the date with <see cref="TimeOnly.MinValue"/> (00:00:00)
    /// and applying the appropriate UTC offset for the specified time zone.
    /// </para>
    /// <para>
    /// This is particularly useful for Azure Table Storage operations where dates need to be converted
    /// to timestamps with proper time zone handling for partition key generation or filtering.
    /// </para>
    /// </remarks>
    public static DateTimeOffset ToDateTimeOffset(this DateOnly dateOnly, TimeZoneInfo? zone = null)
    {
        zone ??= TimeZoneInfo.Local;

        var dateTime = dateOnly.ToDateTime(TimeOnly.MinValue);
        var offset = zone.GetUtcOffset(dateTime);

        return new DateTimeOffset(dateTime, offset);
    }

    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to a <see cref="DateOnly"/> in the specified time zone.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <param name="zone">The time zone to convert to. Uses local time zone if not specified.</param>
    /// <returns>A <see cref="DateOnly"/> representing the date portion in the specified time zone.</returns>
    /// <remarks>
    /// <para>
    /// This method converts the <see cref="DateTimeOffset"/> to the target time zone and extracts
    /// only the date portion, discarding the time component.
    /// </para>
    /// <para>
    /// This is useful when working with Azure Table Storage entities that contain timestamps
    /// but you need to group or filter by date in a specific time zone context.
    /// </para>
    /// <para>
    /// Note that the resulting <see cref="DateOnly"/> represents the calendar date in the target
    /// time zone, which may differ from the UTC date if the conversion crosses a date boundary.
    /// </para>
    /// </remarks>
    public static DateOnly ToDateOnly(this DateTimeOffset dateTime, TimeZoneInfo? zone = null)
    {
        zone ??= TimeZoneInfo.Local;

        var targetZone = TimeZoneInfo.ConvertTime(dateTime, zone);
        return DateOnly.FromDateTime(targetZone.Date);
    }
}
#endif
