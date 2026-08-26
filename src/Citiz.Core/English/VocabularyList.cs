using Citiz.Core.Content;

namespace Citiz.Core.English;

/// <summary>Which part of the English test a vocabulary list is for.</summary>
public enum VocabularyKind
{
    /// <summary>Words the applicant may be asked to read aloud.</summary>
    Reading,

    /// <summary>Words the applicant may be asked to write from dictation.</summary>
    Writing,
}

/// <summary>A group of words under one official heading, e.g. <c>Civics</c> or <c>Holidays</c>.</summary>
/// <param name="Category">The official heading.</param>
/// <param name="Words">The words, as USCIS lists them.</param>
public sealed record VocabularyGroup(string Category, IReadOnlyList<string> Words);

/// <summary>
/// One of the official USCIS vocabulary lists for the reading or writing portion of the English test.
/// </summary>
/// <param name="Kind">Reading or writing.</param>
/// <param name="Groups">The words, grouped under their official headings.</param>
/// <param name="ReviewStatus">Editorial state.</param>
/// <param name="Sources">The official document the list was transcribed from.</param>
public sealed record VocabularyList(
    VocabularyKind Kind,
    IReadOnlyList<VocabularyGroup> Groups,
    ReviewStatus ReviewStatus,
    IReadOnlyList<SourceReference> Sources)
{
    /// <summary>Every word in the list, in official order.</summary>
    public IEnumerable<string> AllWords => Groups.SelectMany(g => g.Words);
}
