using System.Globalization;

namespace Citiz.SharedUI.Services;

/// <summary>Sizes as the interface quotes them before a download.</summary>
public static class AudioFormat
{
    /// <summary>Whole megabytes (decimal, like app stores), never less than 1.</summary>
    public static string Megabytes(long bytes) => Math.Max(1, (long)Math.Round(bytes / 1e6)).ToString(CultureInfo.InvariantCulture);
}
