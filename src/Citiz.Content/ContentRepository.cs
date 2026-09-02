using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Citiz.Content.Files;
using Citiz.Content.Sources;
using Citiz.Core.Audio;
using Citiz.Core.Discovery;
using Citiz.Core.English;
using Citiz.Core.Exams;

namespace Citiz.Content;

/// <summary>
/// The application's view of the content repository: typed, validated, cached per file. Register
/// one per host with the store that fits it (disk on the server and CLI, HTTP in the browser).
/// </summary>
public sealed class ContentRepository
{
    private readonly IContentStore _store;
    private readonly Dictionary<string, Task<object>> _cache = new(StringComparer.Ordinal);
    private readonly Lock _gate = new();

    /// <summary>Creates a repository over <paramref name="store"/>.</summary>
    public ContentRepository(IContentStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }

    /// <summary>The civics-test versions.</summary>
    public Task<IReadOnlyList<ExamVersion>> GetExamVersionsAsync(CancellationToken cancellationToken = default) =>
        CachedAsync(ContentPaths.ExamVersions, ContentJsonContext.Default.ExamVersionsFile, f => ContentMapper.ToExamVersions(f), cancellationToken);

    /// <summary>The question bank of one version.</summary>
    /// <exception cref="FileNotFoundException">No bank exists for that version.</exception>
    public Task<QuestionBank> GetQuestionBankAsync(string versionId, CancellationToken cancellationToken = default)
    {
        var path = ContentPaths.Questions(versionId);
        return CachedAsync(path, ContentJsonContext.Default.QuestionsFile, f => ContentMapper.ToQuestionBank(f, path), cancellationToken);
    }

    /// <summary>The current dynamic answers, keyed by <see cref="DynamicAnswer.Key"/>.</summary>
    public Task<IReadOnlyDictionary<string, DynamicAnswer>> GetDynamicAnswersAsync(CancellationToken cancellationToken = default) =>
        CachedAsync(ContentPaths.DynamicAnswers, ContentJsonContext.Default.DynamicAnswersFile, f => ContentMapper.ToDynamicAnswers(f), cancellationToken);

    /// <summary>The official vocabulary list of one kind.</summary>
    public Task<VocabularyList> GetVocabularyAsync(VocabularyKind kind, CancellationToken cancellationToken = default)
    {
        var path = kind == VocabularyKind.Reading ? ContentPaths.ReadingVocabulary : ContentPaths.WritingVocabulary;
        return CachedAsync(path, ContentJsonContext.Default.VocabularyFile, f => ContentMapper.ToVocabulary(f, path), cancellationToken);
    }

    /// <summary>The discovery capsules.</summary>
    public Task<IReadOnlyList<DiscoveryTopic>> GetDiscoveryTopicsAsync(CancellationToken cancellationToken = default) =>
        CachedAsync(ContentPaths.DiscoveryTopics, ContentJsonContext.Default.DiscoveryTopicsFile, f => ContentMapper.ToDiscoveryTopics(f), cancellationToken);

    /// <summary>The catalog of monitored official sources.</summary>
    public Task<IReadOnlyList<MonitoredSource>> GetMonitoredSourcesAsync(CancellationToken cancellationToken = default) =>
        CachedAsync(ContentPaths.MonitoredSources, ContentJsonContext.Default.SourcesFile, f => ContentMapper.ToMonitoredSources(f), cancellationToken);

    /// <summary>The audio packs the learner can download.</summary>
    public Task<IReadOnlyList<AudioPack>> GetAudioPacksAsync(CancellationToken cancellationToken = default) =>
        CachedAsync(ContentPaths.AudioPacks, ContentJsonContext.Default.AudioPacksFile, f => ContentMapper.ToAudioPacks(f), cancellationToken);

    /// <summary>The bank for the version that applies to an N-400 filing date, or <c>null</c> when no version does.</summary>
    public async Task<(ExamVersion Version, QuestionBank Bank)?> GetExamForFilingDateAsync(DateOnly filingDate, CancellationToken cancellationToken = default)
    {
        var version = ExamPolicy.Resolve(filingDate, await GetExamVersionsAsync(cancellationToken).ConfigureAwait(false));
        if (version is null)
        {
            return null;
        }

        return (version, await GetQuestionBankAsync(version.Id, cancellationToken).ConfigureAwait(false));
    }

    /// <summary>Reads and parses one file without caching or mapping. Used by the validator.</summary>
    /// <exception cref="ContentFormatException">The file is not valid JSON for <typeparamref name="TFile"/>.</exception>
    public async Task<TFile> ReadFileAsync<TFile>(string path, JsonTypeInfo<TFile> typeInfo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);

        await using var stream = await _store.OpenReadAsync(path, cancellationToken).ConfigureAwait(false);
        try
        {
            return await JsonSerializer.DeserializeAsync(stream, typeInfo, cancellationToken).ConfigureAwait(false)
                ?? throw new ContentFormatException(path, "the file is empty.");
        }
        catch (JsonException ex)
        {
            throw new ContentFormatException(path, $"invalid JSON at line {ex.LineNumber}: {ex.Message}", ex);
        }
    }

    /// <summary>Forgets every cached file, so the next call re-reads the store.</summary>
    public void Invalidate()
    {
        lock (_gate)
        {
            _cache.Clear();
        }
    }

    private Task<TResult> CachedAsync<TFile, TResult>(string path, JsonTypeInfo<TFile> typeInfo, Func<TFile, TResult> map, CancellationToken cancellationToken)
        where TResult : class
    {
        Task<object> task;
        lock (_gate)
        {
            if (!_cache.TryGetValue(path, out var cached))
            {
                cached = LoadAsync(path, typeInfo, map, cancellationToken);
                _cache[path] = cached;
            }

            task = cached;
        }

        return Unwrap<TResult>(task);
    }

    private async Task<object> LoadAsync<TFile, TResult>(string path, JsonTypeInfo<TFile> typeInfo, Func<TFile, TResult> map, CancellationToken cancellationToken)
        where TResult : class
    {
        try
        {
            var file = await ReadFileAsync(path, typeInfo, cancellationToken).ConfigureAwait(false);
            return map(file);
        }
        catch
        {
            lock (_gate)
            {
                _cache.Remove(path);
            }

            throw;
        }
    }

    private static async Task<TResult> Unwrap<TResult>(Task<object> task)
        where TResult : class =>
        (TResult)await task.ConfigureAwait(false);
}
