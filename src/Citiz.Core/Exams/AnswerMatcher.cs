using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Citiz.Core.Exams;

/// <summary>How closely a learner's response matched an accepted answer.</summary>
public enum AnswerMatchKind
{
    /// <summary>Nothing in the response corresponds to an accepted answer.</summary>
    None,

    /// <summary>The response resembles an accepted answer but differs by more than punctuation or filler; show it and let the learner judge.</summary>
    Close,

    /// <summary>Every content word of an accepted answer appears in the response.</summary>
    Contains,

    /// <summary>The response is an accepted answer, once punctuation, case and filler are ignored.</summary>
    Exact,
}

/// <summary>Outcome of <see cref="AnswerMatcher.Evaluate"/>.</summary>
/// <param name="Kind">How closely the response matched.</param>
/// <param name="MatchedAnswer">The accepted answer that matched or came closest, in its official form; <c>null</c> for <see cref="AnswerMatchKind.None"/>.</param>
/// <param name="Confidence">0 to 1. <see cref="AnswerMatchKind.Exact"/> is 1; <see cref="AnswerMatchKind.Contains"/> is 0.9; <see cref="AnswerMatchKind.Close"/> is the string similarity.</param>
public sealed record AnswerMatch(AnswerMatchKind Kind, string? MatchedAnswer, double Confidence)
{
    /// <summary>The result for an empty or unrecognised response.</summary>
    public static AnswerMatch None { get; } = new(AnswerMatchKind.None, null, 0);

    /// <summary>Whether the response should be accepted without asking anyone else.</summary>
    public bool IsAccepted => Kind is AnswerMatchKind.Exact or AnswerMatchKind.Contains;
}

/// <summary>
/// The deterministic answer evaluator: the first stage of the design's evaluation flow, and the only
/// stage when no AI provider is configured. It accepts the clear cases and reports the near misses;
/// it never invents an answer. Rules: case, punctuation and hyphens are ignored; parenthesised parts
/// of an official answer are optional, so "(U.S.) Constitution" accepts "Constitution" and
/// "U.S. Constitution"; a purely numeric parenthesis is an alternative on its own, so
/// "Twenty-seven (27)" accepts "27".
/// </summary>
public static partial class AnswerMatcher
{
    private const double CloseThreshold = 0.8;

    private static readonly HashSet<string> FillerWords = new(StringComparer.Ordinal)
    {
        "a", "an", "the", "of", "to", "and", "or", "in", "on", "for", "is", "are", "was", "were", "be", "it", "its", "by", "at", "that", "this",
    };

    /// <summary>Evaluates <paramref name="response"/> against <paramref name="acceptedAnswers"/>, returning the best match.</summary>
    public static AnswerMatch Evaluate(string? response, IEnumerable<string> acceptedAnswers)
    {
        ArgumentNullException.ThrowIfNull(acceptedAnswers);

        var normalizedResponse = Normalize(response);
        if (normalizedResponse.Length == 0)
        {
            return AnswerMatch.None;
        }

        var responseTokens = normalizedResponse.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);
        var responseContent = ContentTokens(normalizedResponse);
        var best = AnswerMatch.None;

        foreach (var accepted in acceptedAnswers)
        {
            foreach (var variant in Variants(accepted))
            {
                var normalizedVariant = Normalize(variant);
                if (normalizedVariant.Length == 0)
                {
                    continue;
                }

                var contentTokens = ContentTokens(normalizedVariant);

                if (normalizedVariant == normalizedResponse ||
                    (contentTokens.Count > 0 && contentTokens.SequenceEqual(responseContent, StringComparer.Ordinal)))
                {
                    return new AnswerMatch(AnswerMatchKind.Exact, accepted, 1);
                }

                if (contentTokens.Count > 0 && contentTokens.All(responseTokens.Contains))
                {
                    best = Better(best, new AnswerMatch(AnswerMatchKind.Contains, accepted, 0.9));
                    continue;
                }

                var similarity = Similarity(normalizedResponse, normalizedVariant);
                if (similarity >= CloseThreshold)
                {
                    best = Better(best, new AnswerMatch(AnswerMatchKind.Close, accepted, similarity));
                }
            }
        }

        return best;
    }

    /// <summary>
    /// The canonical comparison form of a phrase: lower-case, punctuation and hyphens removed,
    /// "United States" folded to "us", whitespace collapsed. Exposed so the interface can show the
    /// learner what was compared.
    /// </summary>
    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var lowered = text.ToLower(CultureInfo.InvariantCulture);
        var builder = new StringBuilder(lowered.Length);
        foreach (var ch in lowered)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
            }
            else if (ch is '.' or '\'' or '’')
            {
                // "U.S." -> "us", "don't" -> "dont": dropped without a space so abbreviations stay one token.
            }
            else
            {
                builder.Append(' ');
            }
        }

        var collapsed = WhitespaceRegex().Replace(builder.ToString(), " ").Trim();
        collapsed = collapsed.Replace("united states of america", "us", StringComparison.Ordinal);
        collapsed = collapsed.Replace("united states", "us", StringComparison.Ordinal);
        return collapsed;
    }

    /// <summary>
    /// The alternative readings of an official answer implied by its parentheses. "(U.S.) Constitution"
    /// yields "U.S. Constitution" and "Constitution"; "Twenty-seven (27)" also yields "27".
    /// </summary>
    public static IReadOnlyList<string> Variants(string acceptedAnswer)
    {
        ArgumentNullException.ThrowIfNull(acceptedAnswer);

        var variants = new List<string> { acceptedAnswer };

        if (!acceptedAnswer.Contains('(', StringComparison.Ordinal))
        {
            return variants;
        }

        variants.Add(Collapse(ParenthesesRegex().Replace(acceptedAnswer, " $1 ")));
        variants.Add(Collapse(ParenthesesRegex().Replace(acceptedAnswer, " ")));

        foreach (Match match in ParenthesesRegex().Matches(acceptedAnswer))
        {
            var inner = match.Groups[1].Value.Trim();
            if (inner.Length > 0 && inner.All(char.IsDigit))
            {
                variants.Add(inner);
            }
        }

        return variants.Where(v => v.Length > 0).Distinct(StringComparer.Ordinal).ToList();
    }

    private static string Collapse(string text) => WhitespaceRegex().Replace(text, " ").Trim();

    private static List<string> ContentTokens(string normalized) =>
        normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(t => !FillerWords.Contains(t)).ToList();

    private static AnswerMatch Better(AnswerMatch current, AnswerMatch candidate) =>
        candidate.Kind > current.Kind || (candidate.Kind == current.Kind && candidate.Confidence > current.Confidence)
            ? candidate
            : current;

    private static double Similarity(string a, string b)
    {
        var longest = Math.Max(a.Length, b.Length);
        return longest == 0 ? 1 : 1 - (double)LevenshteinDistance(a, b) / longest;
    }

    private static int LevenshteinDistance(string a, string b)
    {
        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), previous[j - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[b.Length];
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"\s*\(([^)]*)\)\s*")]
    private static partial Regex ParenthesesRegex();
}
