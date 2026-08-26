namespace Citiz.Core.Content;

/// <summary>
/// Editorial state of a piece of content. Every fact Citiz shows a learner carries one of these, and
/// the interface labels anything that is not <see cref="Approved"/> so nobody studies from an
/// unverified answer without knowing it. See Docs/Editorial/EDR-0002-review-states.md.
/// </summary>
public enum ReviewStatus
{
    /// <summary>Written but not yet checked against its source. Not shown in official modes.</summary>
    Draft,

    /// <summary>Complete, with sources attached, waiting for a content maintainer to verify it.</summary>
    NeedsReview,

    /// <summary>Verified against the cited official source by a content maintainer.</summary>
    Approved,

    /// <summary>Was approved, but the source has changed since. Must be re-verified before use.</summary>
    Outdated,
}

/// <summary>Conversions between <see cref="ReviewStatus"/> and its kebab-case content-file form.</summary>
public static class ReviewStatuses
{
    /// <summary>Parses the content-file form (<c>needs-review</c>, <c>approved</c>, ...).</summary>
    /// <exception cref="FormatException">The value is not a known status.</exception>
    public static ReviewStatus Parse(string value) =>
        TryParse(value, out var status)
            ? status
            : throw new FormatException($"Unknown review status '{value}'. Expected one of: draft, needs-review, approved, outdated.");

    /// <summary>Tries to parse the content-file form, ignoring case.</summary>
    public static bool TryParse(string? value, out ReviewStatus status)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "draft":
                status = ReviewStatus.Draft;
                return true;
            case "needs-review":
                status = ReviewStatus.NeedsReview;
                return true;
            case "approved":
                status = ReviewStatus.Approved;
                return true;
            case "outdated":
                status = ReviewStatus.Outdated;
                return true;
            default:
                status = default;
                return false;
        }
    }

    /// <summary>The kebab-case form used in content files and the interface.</summary>
    public static string ToKebabCase(this ReviewStatus status) => status switch
    {
        ReviewStatus.Draft => "draft",
        ReviewStatus.NeedsReview => "needs-review",
        ReviewStatus.Approved => "approved",
        ReviewStatus.Outdated => "outdated",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };
}
