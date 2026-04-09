namespace PngSharp.Spec.Chunks.tIME;

public readonly record struct TimeChunkData
{
    public ushort Year { get; init; }
    public byte Month { get; init; }
    public byte Day { get; init; }
    public byte Hour { get; init; }
    public byte Minute { get; init; }
    public byte Second { get; init; }

    /// <summary>
    /// Converts to a UTC DateTimeOffset. Leap seconds (60) are clamped to 59.
    /// </summary>
    public DateTimeOffset ToDateTimeOffset()
    {
        var second = Math.Min(Second, (byte)59);
        return new DateTimeOffset(Year, Month, Day, Hour, Minute, second, TimeSpan.Zero);
    }

    /// <summary>
    /// Creates a TimeChunkData from a DateTimeOffset. The value is converted to UTC.
    /// </summary>
    public static TimeChunkData FromDateTimeOffset(DateTimeOffset value)
    {
        var utc = value.UtcDateTime;
        return new TimeChunkData
        {
            Year = (ushort)utc.Year,
            Month = (byte)utc.Month,
            Day = (byte)utc.Day,
            Hour = (byte)utc.Hour,
            Minute = (byte)utc.Minute,
            Second = (byte)utc.Second,
        };
    }
}
