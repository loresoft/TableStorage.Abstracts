namespace TableStorage.Abstracts.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DateTime"/> and <see cref="DateTimeOffset"/> to support Azure Table Storage operations.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Rounds the specified date to the nearest interval defined by the time span.
    /// </summary>
    /// <param name="date">The date to round.</param>
    /// <param name="span">The time span interval to round to (e.g., 5 minutes, 1 hour).</param>
    /// <returns>A <see cref="DateTime"/> rounded to the nearest specified interval.</returns>
    /// <remarks>
    /// <para>
    /// This method performs mathematical rounding to the nearest interval boundary. For example,
    /// rounding to 5-minute intervals will round times like 10:07 to 10:05 and 10:08 to 10:10.
    /// </para>
    /// <para>
    /// This is particularly useful for Azure Table Storage partition key generation where you want
    /// to group entities by time intervals for better performance and distribution.
    /// </para>
    /// </remarks>
    public static DateTime Round(this DateTime date, TimeSpan span)
    {
        long ticks = (date.Ticks + (span.Ticks / 2) + 1) / span.Ticks;
        return new DateTime(ticks * span.Ticks);
    }

    /// <summary>
    /// Rounds the specified date to the nearest interval defined by the time span.
    /// </summary>
    /// <param name="date">The date to round.</param>
    /// <param name="span">The time span interval to round to (e.g., 5 minutes, 1 hour).</param>
    /// <returns>A <see cref="DateTimeOffset"/> rounded to the nearest specified interval, preserving the original offset.</returns>
    /// <remarks>
    /// <para>
    /// This method performs the same rounding logic as the <see cref="DateTime"/> overload but preserves
    /// the original UTC offset of the <see cref="DateTimeOffset"/>.
    /// </para>
    /// <para>
    /// This is particularly useful for Azure Table Storage partition key generation where you want
    /// to group entities by time intervals while maintaining time zone information.
    /// </para>
    /// </remarks>
    public static DateTimeOffset Round(this DateTimeOffset date, TimeSpan span)
    {
        long ticks = (date.Ticks + (span.Ticks / 2) + 1) / span.Ticks;
        return new DateTimeOffset(ticks * span.Ticks, date.Offset);
    }

    /// <summary>
    /// Converts the specified date time to its reverse chronological equivalent for sorting newest items first.
    /// </summary>
    /// <param name="dateTime">The date time to convert.</param>
    /// <returns>A <see cref="DateTime"/> with reverse chronological ordering (DateTime.MaxValue - dateTime).</returns>
    /// <remarks>
    /// <para>
    /// This method creates a reverse chronological timestamp by subtracting the input datetime
    /// from <see cref="DateTime.MaxValue"/>. This results in newer dates having smaller values,
    /// which causes them to sort first in ascending order.
    /// </para>
    /// <para>
    /// This is essential for Azure Table Storage scenarios where you want query results to return
    /// the most recent entities first, as Azure Table Storage sorts entities in ascending order by default.
    /// This technique is commonly used in logging, event sourcing, and time-series data scenarios.
    /// </para>
    /// </remarks>
    public static DateTime ToReverseChronological(this DateTime dateTime)
    {
        var targetTicks = DateTime.MaxValue.Ticks - dateTime.Ticks;
        return new DateTime(targetTicks);
    }

    /// <summary>
    /// Converts the specified date time offset to its reverse chronological equivalent for sorting newest items first.
    /// </summary>
    /// <param name="dateTimeOffset">The date time offset to convert.</param>
    /// <returns>A <see cref="DateTimeOffset"/> with reverse chronological ordering (DateTimeOffset.MaxValue - dateTimeOffset).</returns>
    /// <remarks>
    /// <para>
    /// This method creates a reverse chronological timestamp by subtracting the input datetime offset
    /// from <see cref="DateTimeOffset.MaxValue"/>. The result uses UTC offset (TimeSpan.Zero) to ensure
    /// consistent sorting behavior regardless of the original time zone.
    /// </para>
    /// <para>
    /// This is essential for Azure Table Storage scenarios where you want query results to return
    /// the most recent entities first. By using reverse chronological ordering in partition keys or row keys,
    /// newer entities will appear first in query results since Azure Table Storage sorts in ascending order.
    /// </para>
    /// <para>
    /// This technique is particularly valuable for scenarios like activity feeds, audit logs, or any
    /// time-series data where recent entries are more frequently accessed.
    /// </para>
    /// </remarks>
    public static DateTimeOffset ToReverseChronological(this DateTimeOffset dateTimeOffset)
    {
        var targetTicks = DateTimeOffset.MaxValue.Ticks - dateTimeOffset.Ticks;
        return new DateTimeOffset(targetTicks, TimeSpan.Zero);
    }
}
