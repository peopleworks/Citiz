using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Citiz.Core.Exams;

/// <summary>How closely a learner's response matched an accepted answer.</summary>
public enum AnswerMatchKind
{
    /// <summary>Nothing in the response corresponds to an accepted answer.</summary>
    None,

    /// <summary>The question asks for several items and the response names some of them, but fewer than required.</summary>
    Partial,

    /// <summary>The response resembles an accepted answer but differs by more than punctuation or filler; show it and let the learner judge.</summary>
    Close,

    /// <summary>An accepted answer is in the response, and the only other words are lead-ins ("I think it is…") or an echo of the question.</summary>
    Contains,

    /// <summary>The response is an accepted answer, once punctuation, case and filler are ignored.</summary>
    Exact,
}

/// <summary>Outcome of <see cref="AnswerMatcher.Evaluate"/>.</summary>
/// <param name="Kind">How closely the response matched.</param>
/// <param name="MatchedAnswer">The accepted answer that matched or came closest, in its official form (several, separated by "; ", when the question asks for several items); <c>null</c> for <see cref="AnswerMatchKind.None"/>.</param>
/// <param name="Confidence">0 to 1. <see cref="AnswerMatchKind.Exact"/> is 1; <see cref="AnswerMatchKind.Contains"/> is 0.9; <see cref="AnswerMatchKind.Close"/> is the string similarity; <see cref="AnswerMatchKind.Partial"/> is the share of required items named.</param>
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
/// it never invents an answer, and it never accepts a response that says more than an accepted
/// answer, because "the Vice President", "not the President" and "2 or 6 years" all contain the
/// right words and are all wrong.
/// </summary>
/// <remarks>
/// Rules. Case, punctuation and hyphens are ignored. Number words are read as numbers, so
/// "twenty-seven", "27" and "27th" compare equal, and "twenty-five" is not "five". Parenthesised
/// parts of an official answer are optional, so "(U.S.) Constitution" accepts "Constitution" and
/// "U.S. Constitution"; a numeral in parentheses may stand alone only when the answer is that
/// number ("Twenty-seven (27)", "Six (6) years"), never when it is part of a longer statement
/// ("Citizens eighteen (18) and older" does not accept "18"). A response is accepted when an
/// accepted answer appears in it, in order, and every other word is filler ("the", "of"), a lead-in
/// ("I think it is") or an echo of the question; any other extra word, a negation or a second
/// number makes it at best <see cref="AnswerMatchKind.Close"/>. When the question asks to name
/// several items (<see cref="CivicsQuestion.RequiredCount"/>), that many distinct accepted answers
/// must appear.
/// </remarks>
public static partial class AnswerMatcher
{
    private const double CloseThreshold = 0.8;

    /// <summary>Words that carry no meaning for matching, in the answer or in the response.</summary>
    private static readonly HashSet<string> FillerWords = new(StringComparer.Ordinal)
    {
        "a", "an", "the", "of", "to", "and", "or", "in", "on", "for", "is", "are", "was", "were", "be", "it", "its", "by", "at", "that", "this",
    };

    /// <summary>Words a learner adds around an answer without changing it: "I think it is…", "my answer is…", "because…".</summary>
    private static readonly HashSet<string> LeadInWords = new(StringComparer.Ordinal)
    {
        "i", "think", "believe", "guess", "say", "would", "my", "answer", "maybe", "probably", "id", "im", "he", "she", "they", "we", "there", "because", "mean", "well", "so", "yes", "um", "uh", "hmm", "like", "called", "named",
    };

    /// <summary>Words that reverse or hedge a statement. A response that adds one of these is never accepted.</summary>
    private static readonly HashSet<string> NegationWords = new(StringComparer.Ordinal)
    {
        "not", "no", "never", "non", "nor", "neither", "none", "nothing", "nobody", "without", "except", "unless", "isnt", "arent", "wasnt", "werent", "dont", "doesnt", "didnt", "cant", "cannot", "wont", "wouldnt", "shouldnt", "couldnt", "instead", "rather", "opposite", "false", "wrong", "incorrect",
    };

    private static readonly Dictionary<string, int> NumberWords = new(StringComparer.Ordinal)
    {
        ["zero"] = 0,
        ["one"] = 1,
        ["two"] = 2,
        ["three"] = 3,
        ["four"] = 4,
        ["five"] = 5,
        ["six"] = 6,
        ["seven"] = 7,
        ["eight"] = 8,
        ["nine"] = 9,
        ["ten"] = 10,
        ["eleven"] = 11,
        ["twelve"] = 12,
        ["thirteen"] = 13,
        ["fourteen"] = 14,
        ["fifteen"] = 15,
        ["sixteen"] = 16,
        ["seventeen"] = 17,
        ["eighteen"] = 18,
        ["nineteen"] = 19,
        ["twenty"] = 20,
        ["thirty"] = 30,
        ["forty"] = 40,
        ["fifty"] = 50,
        ["sixty"] = 60,
        ["seventy"] = 70,
        ["eighty"] = 80,
        ["ninety"] = 90,
    };

    /// <summary>Evaluates <paramref name="response"/> against <paramref name="acceptedAnswers"/>, returning the best match.</summary>
    /// <param name="response">What the learner typed or said.</param>
    /// <param name="acceptedAnswers">The official accepted answers, already resolved for dynamic questions.</param>
    /// <param name="prompt">The official question, when known: words echoed from it ("the President is commander in chief") do not count against the response.</param>
    /// <param name="requiredCount">How many distinct accepted answers the question asks for ("Name three…"); 1 for ordinary questions.</param>
    public static AnswerMatch Evaluate(string? response, IEnumerable<string> acceptedAnswers, string? prompt = null, int requiredCount = 1)
    {
        ArgumentNullException.ThrowIfNull(acceptedAnswers);
        ArgumentOutOfRangeException.ThrowIfLessThan(requiredCount, 1);

        var normalizedResponse = Normalize(response);
        if (normalizedResponse.Length == 0)
        {
            return AnswerMatch.None;
        }

        var accepted = acceptedAnswers.ToList();
        var responseTokens = ContentTokens(normalizedResponse);
        var promptTokens = ContentTokens(Normalize(prompt)).ToHashSet(StringComparer.Ordinal);

        // Cover the response with accepted answers: each answer may claim one contiguous run of
        // content tokens, longest reading of any answer first (so "freedom of speech" is claimed
        // whole before "speech" could take part of it), never overlapping another answer's run.
        var covered = new bool[responseTokens.Count];
        var matched = new List<string>();
        var readings = accepted
            .SelectMany(answer => Variants(answer).Select(v => (Answer: answer, Tokens: ContentTokens(Normalize(v)))))
            .Where(r => r.Tokens.Count > 0)
            .OrderByDescending(r => r.Tokens.Count);
        foreach (var (answer, variantTokens) in readings)
        {
            if (matched.Contains(answer, StringComparer.Ordinal))
            {
                continue;
            }

            var start = FindRun(responseTokens, variantTokens, covered);
            if (start >= 0)
            {
                Array.Fill(covered, true, start, variantTokens.Count);
                matched.Add(answer);
            }
        }

        // Saying an answer twice ("Christmas, Christmas and Christmas") is not wrong, just not more
        // items: repeats of a matched answer are covered without counting.
        foreach (var (answer, variantTokens) in readings.Where(r => matched.Contains(r.Answer, StringComparer.Ordinal)))
        {
            for (var start = FindRun(responseTokens, variantTokens, covered); start >= 0; start = FindRun(responseTokens, variantTokens, covered))
            {
                Array.Fill(covered, true, start, variantTokens.Count);
            }
        }

        var uncovered = responseTokens.Where((_, i) => !covered[i]).ToList();
        var negated = uncovered.Any(NegationWords.Contains);
        var extras = uncovered.Where(t => !LeadInWords.Contains(t) && !promptTokens.Contains(t)).ToList();

        if (matched.Count > 0 && !negated && extras.Count == 0)
        {
            if (matched.Count >= requiredCount)
            {
                var matchedAnswer = string.Join("; ", matched);
                return uncovered.Count == 0
                    ? new AnswerMatch(AnswerMatchKind.Exact, matchedAnswer, 1)
                    : new AnswerMatch(AnswerMatchKind.Contains, matchedAnswer, 0.9);
            }

            return new AnswerMatch(AnswerMatchKind.Partial, string.Join("; ", matched), (double)matched.Count / requiredCount);
        }

        // Not accepted: report the closest official answer so the learner can compare.
        var best = AnswerMatch.None;
        foreach (var answer in accepted)
        {
            foreach (var variant in Variants(answer))
            {
                var normalizedVariant = Normalize(variant);
                if (normalizedVariant.Length == 0)
                {
                    continue;
                }

                var similarity = Similarity(normalizedResponse, normalizedVariant);
                if (similarity >= CloseThreshold && similarity > best.Confidence)
                {
                    best = new AnswerMatch(AnswerMatchKind.Close, answer, similarity);
                }
            }
        }

        // An accepted answer with something wrong added ("the Vice President", "not the President",
        // "Theodore Roosevelt") is still the closest thing to show, even when the strings differ a lot.
        if (best.Kind == AnswerMatchKind.None && matched.Count > 0)
        {
            best = new AnswerMatch(AnswerMatchKind.Close, matched[0], CloseThreshold);
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
    /// yields "U.S. Constitution" and "Constitution"; "Twenty-seven (27)" also yields "27", because the
    /// answer is that number. "Citizens eighteen (18) and older" does not yield "18": the number is only
    /// part of the answer.
    /// </summary>
    public static IReadOnlyList<string> Variants(string acceptedAnswer)
    {
        ArgumentNullException.ThrowIfNull(acceptedAnswer);

        var variants = new List<string> { acceptedAnswer };

        // A holiday is named with or without "Day": USCIS itself lists "Christmas" and "Thanksgiving"
        // on the 2008 test and "Christmas Day" and "Thanksgiving Day" on the 2025 test.
        if (HolidayDayRegex().Match(acceptedAnswer) is { Success: true } holiday)
        {
            variants.Add(holiday.Groups[1].Value);
        }

        if (!acceptedAnswer.Contains('(', StringComparison.Ordinal))
        {
            return variants;
        }

        variants.Add(Collapse(ParenthesesRegex().Replace(acceptedAnswer, " $1 ")));
        var withoutParentheses = Collapse(ParenthesesRegex().Replace(acceptedAnswer, " "));
        variants.Add(withoutParentheses);

        var remainingTokens = ContentTokens(Normalize(withoutParentheses));
        foreach (Match match in ParenthesesRegex().Matches(acceptedAnswer))
        {
            var inner = match.Groups[1].Value.Trim();
            if (inner.Length > 0 && inner.All(char.IsDigit) && IsNumberLed(remainingTokens, inner))
            {
                variants.Add(inner);
            }
        }

        return variants.Where(v => v.Length > 0).Distinct(StringComparer.Ordinal).ToList();
    }

    /// <summary>
    /// The words that carry meaning in a normalized phrase: filler removed, number words read as
    /// numerals ("twenty seven" is one token, "27"), ordinal suffixes dropped ("4th" is "4").
    /// </summary>
    public static IReadOnlyList<string> ContentTokens(string normalized)
    {
        ArgumentNullException.ThrowIfNull(normalized);

        var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(t => !FillerWords.Contains(t)).ToList();
        var tokens = new List<string>(words.Count);

        for (var i = 0; i < words.Count; i++)
        {
            if (TryReadNumber(words, i, out var value, out var length))
            {
                tokens.Add(value.ToString(CultureInfo.InvariantCulture));
                i += length - 1;
            }
            else
            {
                tokens.Add(OrdinalRegex().Replace(words[i], "$1"));
            }
        }

        return tokens;
    }

    private static string Collapse(string text) => WhitespaceRegex().Replace(text, " ").Trim();

    /// <summary>Whether a number-only variant is a reading of the answer: the answer starts with that number and has no other.</summary>
    private static bool IsNumberLed(IReadOnlyList<string> answerTokens, string numeral) =>
        answerTokens.Count > 0 &&
        string.Equals(answerTokens[0], numeral, StringComparison.Ordinal) &&
        answerTokens.Skip(1).All(t => !t.All(char.IsDigit));

    /// <summary>Reads a run of number words starting at <paramref name="index"/>: "four hundred thirty five" is 435, "one two" is not a number.</summary>
    private static bool TryReadNumber(IReadOnlyList<string> words, int index, out int value, out int length)
    {
        value = 0;
        length = 0;
        var current = 0;
        var lastMagnitude = int.MaxValue; // the magnitude of the previous word: units/teens 1, tens 10, hundred 100

        for (var i = index; i < words.Count; i++)
        {
            var word = words[i];
            if (string.Equals(word, "hundred", StringComparison.Ordinal))
            {
                if (length == 0 || lastMagnitude >= 100)
                {
                    break;
                }

                current *= 100;
                lastMagnitude = 100;
            }
            else if (NumberWords.TryGetValue(word, out var number))
            {
                var magnitude = number >= 20 ? 10 : 1;
                // A smaller unit may follow a larger one (twenty five, one hundred twenty); the
                // reverse starts a new number (five twenty, one two).
                if (length > 0 && (magnitude >= lastMagnitude || (lastMagnitude == 1 && magnitude == 1)))
                {
                    break;
                }

                current += number;
                lastMagnitude = magnitude;
            }
            else
            {
                break;
            }

            length++;
        }

        value = current;
        return length > 0;
    }

    /// <summary>The first start index where <paramref name="run"/> appears in <paramref name="tokens"/> on uncovered positions, or -1.</summary>
    private static int FindRun(IReadOnlyList<string> tokens, IReadOnlyList<string> run, bool[] covered)
    {
        for (var start = 0; start + run.Count <= tokens.Count; start++)
        {
            var found = true;
            for (var j = 0; j < run.Count; j++)
            {
                if (covered[start + j] || !string.Equals(tokens[start + j], run[j], StringComparison.Ordinal))
                {
                    found = false;
                    break;
                }
            }

            if (found)
            {
                return start;
            }
        }

        return -1;
    }

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

    [GeneratedRegex(@"^(\d+)(st|nd|rd|th)$")]
    private static partial Regex OrdinalRegex();

    [GeneratedRegex(@"^(\S.*\S) Day$", RegexOptions.IgnoreCase)]
    private static partial Regex HolidayDayRegex();
}
