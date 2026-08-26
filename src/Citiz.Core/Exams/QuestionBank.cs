using Citiz.Core.Content;

namespace Citiz.Core.Exams;

/// <summary>The full official question list for one <see cref="ExamVersion"/>, with its provenance.</summary>
public sealed class QuestionBank
{
    private readonly Dictionary<string, CivicsQuestion> _byId;
    private readonly Dictionary<int, CivicsQuestion> _byNumber;

    /// <summary>Creates a bank, indexing questions by id and number.</summary>
    /// <exception cref="ArgumentException">Two questions share an id or a number.</exception>
    public QuestionBank(
        string versionId,
        IReadOnlyList<CivicsQuestion> questions,
        ReviewStatus reviewStatus,
        IReadOnlyList<SourceReference> sources)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versionId);
        ArgumentNullException.ThrowIfNull(questions);
        ArgumentNullException.ThrowIfNull(sources);

        VersionId = versionId;
        Questions = questions;
        ReviewStatus = reviewStatus;
        Sources = sources;

        _byId = new Dictionary<string, CivicsQuestion>(StringComparer.OrdinalIgnoreCase);
        _byNumber = new Dictionary<int, CivicsQuestion>();

        foreach (var question in questions)
        {
            if (!_byId.TryAdd(question.Id, question))
            {
                throw new ArgumentException($"Duplicate question id '{question.Id}' in bank '{versionId}'.", nameof(questions));
            }

            if (!_byNumber.TryAdd(question.Number, question))
            {
                throw new ArgumentException($"Duplicate question number {question.Number} in bank '{versionId}'.", nameof(questions));
            }
        }
    }

    /// <summary>The <see cref="ExamVersion.Id"/> this bank belongs to.</summary>
    public string VersionId { get; }

    /// <summary>Questions in official order.</summary>
    public IReadOnlyList<CivicsQuestion> Questions { get; }

    /// <summary>Editorial state of the bank as a whole; individual questions may be further along.</summary>
    public ReviewStatus ReviewStatus { get; }

    /// <summary>The official document(s) the bank was transcribed from.</summary>
    public IReadOnlyList<SourceReference> Sources { get; }

    /// <summary>Number of questions in the bank.</summary>
    public int Count => Questions.Count;

    /// <summary>Distinct top-level categories, in order of first appearance.</summary>
    public IReadOnlyList<string> Categories => Questions.Select(q => q.Category).Distinct().ToList();

    /// <summary>Distinct subcategories, in order of first appearance.</summary>
    public IReadOnlyList<string> Subcategories => Questions.Select(q => q.Subcategory).Distinct().ToList();

    /// <summary>Finds a question by id, or <c>null</c>.</summary>
    public CivicsQuestion? Find(string id) => _byId.GetValueOrDefault(id);

    /// <summary>Finds a question by official number, or <c>null</c>.</summary>
    public CivicsQuestion? FindByNumber(int number) => _byNumber.GetValueOrDefault(number);

    /// <summary>Questions in a top-level category or subcategory (case-insensitive).</summary>
    public IReadOnlyList<CivicsQuestion> InCategory(string category) =>
        Questions
            .Where(q => string.Equals(q.Category, category, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(q.Subcategory, category, StringComparison.OrdinalIgnoreCase))
            .ToList();

    /// <summary>
    /// The questions designated for the 65/20 special consideration in <paramref name="version"/>.
    /// Empty when the designation has not been recorded yet; callers must not fall back to the full bank.
    /// </summary>
    public IReadOnlyList<CivicsQuestion> SeniorQuestions(ExamVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);
        return version.SeniorQuestionNumbers
            .Select(FindByNumber)
            .OfType<CivicsQuestion>()
            .ToList();
    }
}
