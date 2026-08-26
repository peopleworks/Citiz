using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Citiz.Content.Sources;

namespace Citiz.ContentWorker.Monitoring;

/// <summary>The outcome of checking one source.</summary>
/// <param name="Source">The source checked.</param>
/// <param name="Hash">SHA-256 of the normalized document, or <c>null</c> when the fetch failed.</param>
/// <param name="Changed">Whether the hash differs from the previous one.</param>
/// <param name="Error">Why the fetch failed, if it did.</param>
public sealed record SourceCheckResult(MonitoredSource Source, string? Hash, bool Changed, string? Error)
{
    /// <summary>Whether the document was fetched.</summary>
    public bool Succeeded => Hash is not null;
}

/// <summary>
/// Fetches a monitored source and reduces it to a hash that ignores the noise official sites add on
/// every request (whitespace, script tags, timestamps in comments), so a change in the hash is a
/// change worth a human's attention.
/// </summary>
public sealed partial class SourceMonitor
{
    private readonly HttpClient _http;

    /// <summary>Creates the monitor over <paramref name="http"/>.</summary>
    public SourceMonitor(HttpClient http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    /// <summary>Fetches <paramref name="source"/> and compares it with <paramref name="previousHash"/>.</summary>
    public async Task<SourceCheckResult> CheckAsync(MonitoredSource source, string? previousHash, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);

        try
        {
            using var response = await _http.GetAsync(source.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new SourceCheckResult(source, null, false, $"HTTP {(int)response.StatusCode}");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var hash = source.Format == "html" ? HashHtml(bytes) : HashBytes(bytes);
            return new SourceCheckResult(source, hash, previousHash is not null && !string.Equals(previousHash, hash, StringComparison.Ordinal), null);
        }
        catch (HttpRequestException ex)
        {
            return new SourceCheckResult(source, null, false, ex.Message);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new SourceCheckResult(source, null, false, "timed out");
        }
    }

    /// <summary>Hashes an HTML document after stripping scripts, styles, comments and whitespace runs.</summary>
    public static string HashHtml(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        var html = Encoding.UTF8.GetString(bytes);
        html = ScriptStyleRegex().Replace(html, string.Empty);
        html = CommentRegex().Replace(html, string.Empty);
        html = WhitespaceRegex().Replace(html, " ").Trim();
        return HashBytes(Encoding.UTF8.GetBytes(html));
    }

    /// <summary>Lower-case hex SHA-256.</summary>
    public static string HashBytes(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        return Convert.ToHexStringLower(SHA256.HashData(bytes));
    }

    [GeneratedRegex(@"<(script|style)\b[^>]*>.*?</\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptStyleRegex();

    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex CommentRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
