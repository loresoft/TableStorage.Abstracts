using System;
using System.Collections.Generic;
using System.Text;

namespace TableStorage.Abstracts.Extensions;

public static class DateTimeExtentions
{
    /// <summary>
    /// Rounds the date to the specified time span.
    /// </summary>
    /// <param name="date">The date to round.</param>
    /// <param name="span">The time span to round to.</param>
    /// <returns>The rounded date</returns>
    public static DateTime Round(this DateTime date, TimeSpan span)
    {
        long ticks = (date.Ticks + (span.Ticks / 2) + 1) / span.Ticks;
        return new DateTime(ticks * span.Ticks);
    }

    /// <summary>
    /// Rounds the date to the specified span.
    /// </summary>
    /// <param name="date">The date to round.</param>
    /// <param name="span">The time span to round to.</param>
    /// <returns>The rounded date</returns>
    public static DateTimeOffset Round(this DateTimeOffset date, TimeSpan span)
    {
        long ticks = (date.Ticks + (span.Ticks / 2) + 1) / span.Ticks;
        return new DateTimeOffset(ticks * span.Ticks, date.Offset);
    }

    /// <summary>
    /// Converts to specified <paramref name="dateTime"/> to its reverse chronological equivalent. DateTime.MaxValue - dateTime
    /// </summary>
    /// <param name="dateTime">The date time offset.</param>
    /// <returns>A <see cref="DateTime"/> chronological reversed.</returns>
    public static DateTime ToReverseChronological(this DateTime dateTime)
    {
        var targetTicks = DateTime.MaxValue.Ticks - dateTime.Ticks;
        return new DateTime(targetTicks);
    }

    /// <summary>
    /// Converts to specified <paramref name="dateTimeOffset"/> to its reverse chronological equivalent. DateTimeOffset.MaxValue - dateTimeOffset
    /// </summary>
    /// <param name="dateTimeOffset">The date time offset.</param>
    /// <returns>A <see cref="DateTimeOffset"/> chronological reversed.</returns>
    public static DateTimeOffset ToReverseChronological(this DateTimeOffset dateTimeOffset)
    {
        var targetTicks = DateTimeOffset.MaxValue.Ticks - dateTimeOffset.Ticks;
        return new DateTimeOffset(targetTicks, TimeSpan.Zero);
    }
}
