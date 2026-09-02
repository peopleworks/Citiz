using System.Globalization;
using Citiz.Content.Sources;
using Citiz.Core.Audio;
using Citiz.Core.Content;
using Citiz.Core.Discovery;
using Citiz.Core.English;
using Citiz.Core.Exams;

namespace Citiz.Content.Validation;

/// <summary>How serious a validation finding is.</summary>
public enum ContentIssueSeverity
{
    /// <summary>Worth knowing, nothing to fix.</summary>
    Info,

    /// <summary>Content loads, but something is incomplete (typically: not yet verified).</summary>
    Warning,

    /// <summary>Content is wrong or unusable; the pull request must not merge.</summary>
    Error,
}

/// <summary>One validation finding.</summary>
/// <param name="Severity">How serious it is.</param>
/// <param name="File">Content-relative path of the file.</param>
/// <param name="Message">What is wrong, naming the entry.</param>
public sealed record ContentIssue(ContentIssueSeverity Severity, string File, string Message);

/// <summary>How many entries of a file are in each review state.</summary>
/// <param name="File">Content-relative path.</param>
/// <param name="Counts">Entries per status.</param>
public sealed record ReviewSummary(string File, IReadOnlyDictionary<ReviewStatus, int> Counts)
{
    /// <summary>Total entries.</summary>
    public int Total => Counts.Values.Sum();

    /// <summary>Entries not yet <see cref="ReviewStatus.Approved"/>.</summary>
    public int Pending => Total - Counts.GetValueOrDefault(ReviewStatus.Approved);
}

/// <summary>Everything the content validator found, plus the loaded content when it loaded at all.</summary>
public sealed class ContentValidationReport
{
    internal ContentValidationReport(IReadOnlyList<ContentIssue> issues, IReadOnlyList<ReviewSummary> reviews)
    {
        Issues = issues;
        Reviews = reviews;
    }

    /// <summary>Findings, errors first.</summary>
    public IReadOnlyList<ContentIssue> Issues { get; }

    /// <summary>Review-state counts per file.</summary>
    public IReadOnlyList<ReviewSummary> Reviews { get; }

    /// <summary>Whether there are no errors.</summary>
    public bool IsValid => Issues.All(i => i.Severity != ContentIssueSeverity.Error);

    /// <summary>Number of errors.</summary>
    public int ErrorCount => Issues.Count(i => i.Severity == ContentIssueSeverity.Error);

    /// <summary>Number of warnings.</summary>
    public int WarningCount => Issues.Count(i => i.Severity == ContentIssueSeverity.Warning);
}

/// <summary>
/// Checks the content repository as a whole: every file parses and maps, cross-references resolve,
/// exam rules are consistent, and nothing an officer would ask is missing. Runs in CI on every pull
/// request and behind <c>citiz content validate</c>.
/// </summary>
public sealed class ContentValidator
{
    private readonly ContentRepository _repository;
    private readonly IContentStore _store;
    private readonly List<ContentIssue> _issues = [];
    private readonly List<ReviewSummary> _reviews = [];

    /// <summary>Creates a validator over <paramref name="store"/>.</summary>
    public ContentValidator(IContentStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
        _repository = new ContentRepository(store);
    }

    /// <summary>Runs every check and returns the report. Never throws for content problems; they become issues.</summary>
    public async Task<ContentValidationReport> ValidateAsync(CancellationToken cancellationToken = default)
    {
        _issues.Clear();
        _reviews.Clear();

        var versions = await LoadAsync(ContentPaths.ExamVersions, () => _repository.GetExamVersionsAsync(cancellationToken)).ConfigureAwait(false);
        var dynamicAnswers = await LoadAsync(ContentPaths.DynamicAnswers, () => _repository.GetDynamicAnswersAsync(cancellationToken)).ConfigureAwait(false);
        var banks = new Dictionary<string, QuestionBank>(StringComparer.Ordinal);

        if (versions is not null)
        {
            CheckVersions(versions);
            foreach (var version in versions)
            {
                var path = ContentPaths.Questions(version.Id);
                var bank = await LoadAsync(path, () => _repository.GetQuestionBankAsync(version.Id, cancellationToken)).ConfigureAwait(false);
                if (bank is not null)
                {
                    banks[version.Id] = bank;
                    CheckBank(version, bank, dynamicAnswers, path);
                }
            }
        }

        if (dynamicAnswers is not null)
        {
            CheckDynamicAnswers(dynamicAnswers, banks.Values);
        }

        var vocabularies = new List<VocabularyList>();
        foreach (var kind in new[] { VocabularyKind.Reading, VocabularyKind.Writing })
        {
            var path = kind == VocabularyKind.Reading ? ContentPaths.ReadingVocabulary : ContentPaths.WritingVocabulary;
            var list = await LoadAsync(path, () => _repository.GetVocabularyAsync(kind, cancellationToken)).ConfigureAwait(false);
            if (list is not null)
            {
                vocabularies.Add(list);
                CheckVocabulary(list, kind, path);
            }
        }

        var topics = await LoadAsync(ContentPaths.DiscoveryTopics, () => _repository.GetDiscoveryTopicsAsync(cancellationToken)).ConfigureAwait(false);
        if (topics is not null)
        {
            CheckTopics(topics, banks);
        }

        var sources = await LoadAsync(ContentPaths.MonitoredSources, () => _repository.GetMonitoredSourcesAsync(cancellationToken)).ConfigureAwait(false);
        if (sources is not null)
        {
            await CheckMonitoredSourcesAsync(sources, cancellationToken).ConfigureAwait(false);
        }

        var packs = await LoadAsync(ContentPaths.AudioPacks, () => _repository.GetAudioPacksAsync(cancellationToken)).ConfigureAwait(false);
        if (packs is not null)
        {
            CheckAudioPacks(packs, banks, vocabularies);
        }

        var ordered = _issues.OrderByDescending(i => i.Severity).ThenBy(i => i.File, StringComparer.Ordinal).ToList();
        return new ContentValidationReport(ordered, _reviews.ToList());
    }

    private async Task<T?> LoadAsync<T>(string path, Func<Task<T>> load)
        where T : class
    {
        try
        {
            return await load().ConfigureAwait(false);
        }
        catch (ContentFormatException ex)
        {
            Error(path, ex.Message[(ex.File.Length + 2)..]);
        }
        catch (FileNotFoundException)
        {
            Error(path, "file is missing.");
        }

        return null;
    }

    private void CheckVersions(IReadOnlyList<ExamVersion> versions)
    {
        const string path = ContentPaths.ExamVersions;

        if (versions.Count == 0)
        {
            Error(path, "no exam versions are defined.");
            return;
        }

        foreach (var duplicate in versions.GroupBy(v => v.Id, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
        {
            Error(path, $"version id '{duplicate.Key}' appears more than once.");
        }

        for (var i = 0; i < versions.Count; i++)
        {
            for (var j = i + 1; j < versions.Count; j++)
            {
                if (Overlaps(versions[i], versions[j]))
                {
                    Error(path, $"versions '{versions[i].Id}' and '{versions[j].Id}' both apply to some filing dates; their filing ranges overlap.");
                }
            }
        }

        if (versions.Count(v => v.IsCurrent) != 1)
        {
            Error(path, "exactly one version must have filingTo = null (the version currently given to new applicants).");
        }

        foreach (var version in versions)
        {
            if (version.Sources.Count == 0)
            {
                Error(path, $"version '{version.Id}' has no sources.");
            }

            if (!version.HasSeniorDesignation)
            {
                Warning(path, $"version '{version.Id}' has no 65/20 question list yet (seniorQuestionNumbers is empty); the 65/20 mode is disabled for it.");
            }
            else if (version.SeniorQuestionNumbers.Count < version.SeniorConsideration.QuestionsAsked)
            {
                Error(path, $"version '{version.Id}' designates {version.SeniorQuestionNumbers.Count} 65/20 questions but asks {version.SeniorConsideration.QuestionsAsked}.");
            }

            if (version.FilingFrom is { } from && version.FilingTo is { } to && from > to)
            {
                Error(path, $"version '{version.Id}' has filingFrom after filingTo.");
            }
        }

        Summarize(path, versions.Select(v => v.ReviewStatus));
    }

    private static bool Overlaps(ExamVersion a, ExamVersion b)
    {
        var aFrom = a.FilingFrom ?? DateOnly.MinValue;
        var aTo = a.FilingTo ?? DateOnly.MaxValue;
        var bFrom = b.FilingFrom ?? DateOnly.MinValue;
        var bTo = b.FilingTo ?? DateOnly.MaxValue;
        return aFrom <= bTo && bFrom <= aTo;
    }

    private void CheckBank(ExamVersion version, QuestionBank bank, IReadOnlyDictionary<string, DynamicAnswer>? dynamicAnswers, string path)
    {
        if (bank.Count != version.BankSize)
        {
            Error(path, $"bank has {bank.Count} questions but versions.json says bankSize {version.BankSize}.");
        }

        if (bank.Sources.Count == 0)
        {
            Error(path, "the bank has no sources.");
        }

        var expected = 1;
        foreach (var question in bank.Questions)
        {
            var where = $"question '{question.Id}'";

            if (question.Number != expected)
            {
                Error(path, $"{where} has number {question.Number}; expected {expected} (numbers must be contiguous and in order).");
            }

            expected = question.Number + 1;

            var expectedId = $"{bank.VersionId}-{question.Number:000}";
            if (!string.Equals(question.Id, expectedId, StringComparison.Ordinal))
            {
                Error(path, $"{where} should have id '{expectedId}'.");
            }

            if (question.IsDynamic)
            {
                if (dynamicAnswers is not null && !dynamicAnswers.ContainsKey(question.DynamicAnswerKey!))
                {
                    Error(path, $"{where} references dynamic answer key '{question.DynamicAnswerKey}', which is not defined in {ContentPaths.DynamicAnswers}.");
                }
            }
            else if (question.AcceptedAnswers.Count == 0)
            {
                Error(path, $"{where} has no accepted answers and no dynamicAnswerKey.");
            }

            var duplicates = question.AcceptedAnswers
                .GroupBy(a => AnswerMatcher.Normalize(a), StringComparer.Ordinal)
                .Where(g => g.Count() > 1)
                .Select(g => g.First());
            foreach (var duplicate in duplicates)
            {
                Warning(path, $"{where} lists '{duplicate}' more than once (after normalization).");
            }
        }

        foreach (var number in version.SeniorQuestionNumbers.Where(n => bank.FindByNumber(n) is null))
        {
            Error(ContentPaths.ExamVersions, $"version '{version.Id}' designates 65/20 question {number}, which does not exist in the bank.");
        }

        Summarize(path, bank.Questions.Select(q => q.ReviewStatus));
    }

    private void CheckDynamicAnswers(IReadOnlyDictionary<string, DynamicAnswer> answers, IEnumerable<QuestionBank> banks)
    {
        const string path = ContentPaths.DynamicAnswers;
        var referenced = banks.SelectMany(b => b.Questions).Where(q => q.IsDynamic).Select(q => q.DynamicAnswerKey!).ToHashSet(StringComparer.Ordinal);

        foreach (var answer in answers.Values)
        {
            var where = $"answer '{answer.Key}'";

            if (answer.Scope == DynamicAnswerScope.Federal)
            {
                if (!answer.IsResolved)
                {
                    Warning(path, $"{where} is federal but has no holder or accepted answers; the interface will say the answer is unavailable.");
                }

                if (answer.Sources.Count == 0)
                {
                    Error(path, $"{where} has no sources.");
                }
            }
            else if (answer.LookupHint is null)
            {
                Warning(path, $"{where} varies by learner but gives no lookupHint.");
            }

            if (!referenced.Contains(answer.Key))
            {
                Info(path, $"{where} is not referenced by any question.");
            }
        }

        Summarize(path, answers.Values.Select(a => a.ReviewStatus));
    }

    private void CheckVocabulary(VocabularyList list, VocabularyKind kind, string path)
    {
        if (list.Kind != kind)
        {
            Error(path, $"kind is '{list.Kind}' but the file is the {kind} list.");
        }

        if (list.Groups.Count == 0)
        {
            Error(path, "no vocabulary groups.");
        }

        if (list.Sources.Count == 0)
        {
            Error(path, "no sources.");
        }

        foreach (var group in list.Groups.Where(g => g.Words.Count == 0))
        {
            Error(path, $"group '{group.Category}' has no words.");
        }

        // The official lists repeat a word across headings ("Washington" is both a person and a
        // place), so only a repeat within one heading is suspicious.
        foreach (var group in list.Groups)
        {
            foreach (var duplicate in group.Words.GroupBy(w => w, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
            {
                Warning(path, $"'{duplicate.Key}' appears more than once under '{group.Category}'.");
            }
        }

        Summarize(path, [list.ReviewStatus]);
    }

    private void CheckTopics(IReadOnlyList<DiscoveryTopic> topics, IReadOnlyDictionary<string, QuestionBank> banks)
    {
        const string path = ContentPaths.DiscoveryTopics;
        var categories = new HashSet<string>(StringComparer.Ordinal) { "history", "geography", "people", "institutions", "culture", "innovation", "nature" };
        var difficulties = new HashSet<string>(StringComparer.Ordinal) { "beginner", "intermediate", "advanced" };
        var questionIds = banks.Values.SelectMany(b => b.Questions).Select(q => q.Id).ToHashSet(StringComparer.Ordinal);

        foreach (var duplicate in topics.GroupBy(t => t.Id, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            Error(path, $"topic id '{duplicate.Key}' appears more than once.");
        }

        foreach (var topic in topics)
        {
            var where = $"topic '{topic.Id}'";

            if (!categories.Contains(topic.Category))
            {
                Error(path, $"{where} category '{topic.Category}' is not one of {string.Join(", ", categories)}.");
            }

            if (!difficulties.Contains(topic.Difficulty))
            {
                Error(path, $"{where} difficulty '{topic.Difficulty}' is not one of {string.Join(", ", difficulties)}.");
            }

            if (topic.EstimatedMinutes > 10)
            {
                Warning(path, $"{where} estimates {topic.EstimatedMinutes} minutes; capsules are meant to take 10 or fewer.");
            }

            if (topic.Sources.Count == 0)
            {
                Error(path, $"{where} has no sources.");
            }

            foreach (var id in topic.RelatedQuestionIds.Where(id => banks.Count > 0 && !questionIds.Contains(id)))
            {
                Error(path, $"{where} relates to question '{id}', which does not exist in any bank.");
            }
        }

        Summarize(path, topics.Select(t => t.ReviewStatus));
    }

    private async Task CheckMonitoredSourcesAsync(IReadOnlyList<MonitoredSource> sources, CancellationToken cancellationToken)
    {
        const string path = ContentPaths.MonitoredSources;

        foreach (var duplicate in sources.GroupBy(s => s.Id, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            Error(path, $"source id '{duplicate.Key}' appears more than once.");
        }

        foreach (var source in sources)
        {
            foreach (var feed in source.Feeds)
            {
                if (!await _store.ExistsAsync(feed, cancellationToken).ConfigureAwait(false))
                {
                    Warning(path, $"source '{source.Id}' feeds '{feed}', which does not exist.");
                }
            }
        }
    }

    /// <summary>
    /// Audio packs must point at real content: every clip names a question that exists in the pack's
    /// version, an answer index inside that question's accepted answers, or a word in a vocabulary
    /// list; official packs carry recordings only, synthetic packs never do; and the totals add up,
    /// because the interface quotes them before a download.
    /// </summary>
    private void CheckAudioPacks(IReadOnlyList<AudioPack> packs, IReadOnlyDictionary<string, QuestionBank> banks, IReadOnlyList<VocabularyList> vocabularies)
    {
        const string path = ContentPaths.AudioPacks;
        var words = vocabularies.SelectMany(v => v.AllWords).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var clipIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var duplicate in packs.GroupBy(p => p.Id, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            Error(path, $"pack id '{duplicate.Key}' appears more than once.");
        }

        foreach (var pack in packs)
        {
            var where = $"pack '{pack.Id}'";
            var bank = pack.VersionId is { } versionId && banks.TryGetValue(versionId, out var b) ? b : null;

            if (pack.VersionId is not null && bank is null)
            {
                Error(path, $"{where} names version '{pack.VersionId}', which has no question bank.");
            }

            if (pack.Kind == AudioPackKind.Synthetic && pack.Voice is null)
            {
                Warning(path, $"{where} is synthetic but does not say which voice generated it.");
            }

            if (pack.Sources.Count == 0)
            {
                Error(path, $"{where} has no sources.");
            }

            var total = pack.Clips.Sum(c => c.Bytes);
            if (total != pack.SizeBytes)
            {
                Error(path, $"{where} sizeBytes is {pack.SizeBytes.ToString(CultureInfo.InvariantCulture)} but its clips add up to {total.ToString(CultureInfo.InvariantCulture)}.");
            }

            foreach (var duplicate in pack.Clips.GroupBy(c => c.File, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
            {
                Error(path, $"{where} lists file '{duplicate.Key}' more than once.");
            }

            foreach (var clip in pack.Clips)
            {
                var clipWhere = $"{where} clip '{clip.Id}'";

                if (!clipIds.Add(clip.Id))
                {
                    Error(path, $"{clipWhere} id appears more than once across packs.");
                }

                if (clip.Sha256.Length != 64 || !clip.Sha256.All(Uri.IsHexDigit))
                {
                    Error(path, $"{clipWhere} sha256 is not a 64-digit hex digest.");
                }

                var officialRole = clip.Role == AudioClipRole.Recording;
                if (pack.Kind == AudioPackKind.Official != officialRole)
                {
                    Error(path, $"{clipWhere} role '{clip.Role}' does not fit a {pack.Kind.ToString().ToLowerInvariant()} pack: official packs hold recordings, synthetic packs hold prompts, answers and words.");
                }

                switch (clip.Role)
                {
                    case AudioClipRole.Recording:
                    case AudioClipRole.Prompt:
                    case AudioClipRole.Answer:
                        var question = clip.QuestionId is { } qid && bank is not null ? bank.Questions.FirstOrDefault(q => q.Id == qid) : null;
                        if (clip.QuestionId is null)
                        {
                            Error(path, $"{clipWhere} has no questionId.");
                        }
                        else if (question is null)
                        {
                            Error(path, $"{clipWhere} names question '{clip.QuestionId}', which is not in the bank of version '{pack.VersionId}'.");
                        }
                        else if (clip.Role == AudioClipRole.Answer)
                        {
                            if (clip.AnswerIndex is not { } index)
                            {
                                Error(path, $"{clipWhere} has no answerIndex.");
                            }
                            else if (question.IsDynamic)
                            {
                                Error(path, $"{clipWhere} voices an answer of dynamic question '{question.Id}'; dynamic answers change and are never recorded.");
                            }
                            else if (index >= question.AcceptedAnswers.Count)
                            {
                                Error(path, $"{clipWhere} answerIndex {index.ToString(CultureInfo.InvariantCulture)} is outside the {question.AcceptedAnswers.Count.ToString(CultureInfo.InvariantCulture)} accepted answers of '{question.Id}'.");
                            }
                        }

                        break;

                    case AudioClipRole.Word:
                        if (clip.Word is null)
                        {
                            Error(path, $"{clipWhere} has no word.");
                        }
                        else if (!words.Contains(clip.Word))
                        {
                            Error(path, $"{clipWhere} voices '{clip.Word}', which is not in the reading or writing vocabulary.");
                        }

                        break;
                }
            }
        }

        Summarize(path, packs.Select(p => p.ReviewStatus));
    }

    private void Summarize(string path, IEnumerable<ReviewStatus> statuses)
    {
        var counts = statuses.GroupBy(s => s).ToDictionary(g => g.Key, g => g.Count());
        _reviews.Add(new ReviewSummary(path, counts));
    }

    private void Error(string file, string message) => _issues.Add(new ContentIssue(ContentIssueSeverity.Error, file, message));

    private void Warning(string file, string message) => _issues.Add(new ContentIssue(ContentIssueSeverity.Warning, file, message));

    private void Info(string file, string message) => _issues.Add(new ContentIssue(ContentIssueSeverity.Info, file, message));
}
