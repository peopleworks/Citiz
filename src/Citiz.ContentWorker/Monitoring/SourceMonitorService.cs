using Citiz.Content;
using Microsoft.Extensions.Options;

namespace Citiz.ContentWorker.Monitoring;

/// <summary>Settings for the monitoring loop (<c>Monitoring</c> configuration section).</summary>
public sealed class MonitoringOptions
{
    /// <summary>How often the loop wakes up to see which sources are due. Each source also has its own <c>checkEvery</c>.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(6);

    /// <summary>Run every source once at startup regardless of its schedule.</summary>
    public bool CheckOnStartup { get; set; } = true;
}

/// <summary>
/// The loop: for every monitored source that is due, fetch it, hash it and log whether it changed
/// since the last observation. Observations live in memory for the life of the process; writing
/// them back to <c>sources.json</c> and opening a review is the next step on the roadmap.
/// </summary>
public sealed class SourceMonitorService : BackgroundService
{
    private readonly ContentRepository _content;
    private readonly SourceMonitor _monitor;
    private readonly MonitoringOptions _options;
    private readonly ILogger<SourceMonitorService> _logger;
    private readonly Dictionary<string, (string Hash, DateTimeOffset CheckedAt)> _observations = new(StringComparer.Ordinal);

    /// <summary>Creates the service.</summary>
    public SourceMonitorService(ContentRepository content, SourceMonitor monitor, IOptions<MonitoringOptions> options, ILogger<SourceMonitorService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _content = content;
        _monitor = monitor;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var first = true;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(first && _options.CheckOnStartup, stoppingToken);
            }
            catch (ContentFormatException ex)
            {
                _logger.LogError(ex, "The source catalog could not be read; fix it and restart.");
            }

            first = false;
            await Task.Delay(_options.Interval, stoppingToken);
        }
    }

    private async Task RunOnceAsync(bool force, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var sources = await _content.GetMonitoredSourcesAsync(cancellationToken);

        foreach (var source in sources.Where(s => s.Monitor))
        {
            var previous = _observations.TryGetValue(source.Id, out var observed) ? observed : default;
            var previousHash = previous.Hash ?? source.LastHash;
            DateTimeOffset? lastChecked = previous.Hash is not null
                ? previous.CheckedAt
                : source.LastCheckedOn is { } date ? new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero) : null;

            if (!force && lastChecked is { } checkedAt && checkedAt + source.CheckEvery > now)
            {
                continue;
            }

            var result = await _monitor.CheckAsync(source, previousHash, cancellationToken);

            if (!result.Succeeded)
            {
                _logger.LogWarning("{SourceId} ({Url}): could not fetch: {Error}", source.Id, source.Url, result.Error);
                continue;
            }

            _observations[source.Id] = (result.Hash!, now);

            if (result.Changed)
            {
                _logger.LogWarning(
                    "{SourceId} ({Url}) CHANGED. Re-verify: {Feeds}. Human review required: {HumanReview}.",
                    source.Id,
                    source.Url,
                    string.Join(", ", source.Feeds),
                    source.RequiresHumanReview);
            }
            else
            {
                _logger.LogInformation("{SourceId}: {State} (sha256 {Hash})", source.Id, previousHash is null ? "first observation" : "unchanged", result.Hash![..12]);
            }
        }
    }
}
