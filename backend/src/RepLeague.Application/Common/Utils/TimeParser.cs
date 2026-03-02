namespace RepLeague.Application.Common.Utils;

public static class TimeParser
{
    /// <summary>Parses "mm:ss" or "hh:mm:ss" to total seconds. Returns null on failure.</summary>
    public static int? ParseToSeconds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return TimeSpan.TryParse(value, out var ts) ? (int)ts.TotalSeconds : null;
    }

    /// <summary>Formats total seconds to "mm:ss" (or "h:mm:ss" when >= 1 hour).</summary>
    public static string? FormatSeconds(int? totalSeconds)
    {
        if (totalSeconds == null) return null;
        var ts = TimeSpan.FromSeconds(totalSeconds.Value);
        return ts.TotalHours >= 1
            ? ts.ToString(@"h\:mm\:ss")
            : ts.ToString(@"mm\:ss");
    }
}
