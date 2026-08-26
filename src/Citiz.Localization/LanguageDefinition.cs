namespace Citiz.Localization;

/// <summary>Writing direction of a language.</summary>
public enum TextDirection
{
    /// <summary>Left to right.</summary>
    LeftToRight,

    /// <summary>Right to left, e.g. Arabic.</summary>
    RightToLeft,
}

/// <summary>How far a language pack has been reviewed. Shown in the interface, so nobody mistakes a machine draft for a reviewed translation.</summary>
public enum TranslationReviewStatus
{
    /// <summary>The reference language the others are translated from.</summary>
    Source,

    /// <summary>Reviewed by a fluent speaker.</summary>
    Reviewed,

    /// <summary>Written by a contributor, not yet reviewed by a second speaker.</summary>
    Draft,

    /// <summary>Produced by machine translation or an AI model, unreviewed.</summary>
    MachineDraft,
}

/// <summary>One interface language.</summary>
/// <param name="Code">BCP 47 tag used for the file name and <c>lang</c> attribute, e.g. <c>zh-Hant</c>.</param>
/// <param name="EnglishName">Name in English.</param>
/// <param name="NativeName">Name in the language itself.</param>
/// <param name="Direction">Writing direction.</param>
/// <param name="Status">Review state of the pack.</param>
public sealed record LanguageDefinition(
    string Code,
    string EnglishName,
    string NativeName,
    TextDirection Direction,
    TranslationReviewStatus Status)
{
    /// <summary>The HTML <c>dir</c> attribute value.</summary>
    public string HtmlDirection => Direction == TextDirection.RightToLeft ? "rtl" : "ltr";
}
