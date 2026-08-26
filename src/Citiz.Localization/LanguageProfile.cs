namespace Citiz.Localization;

/// <summary>
/// The learner's three independent language choices (ADR-0002): the interface can be in Spanish,
/// the practice in English, and the explanations in Vietnamese. A single culture cannot express
/// that, and collapsing them would let a navigation change alter official content.
/// </summary>
/// <param name="InterfaceCulture">Language of navigation, buttons and notices.</param>
/// <param name="StudyCulture">Language being practised; English for the naturalization interview.</param>
/// <param name="HelpCulture">Language of explanations and educational support.</param>
public sealed record LanguageProfile(string InterfaceCulture, string StudyCulture, string HelpCulture)
{
    /// <summary>English everywhere.</summary>
    public static LanguageProfile Default { get; } = new(SupportedLanguages.Fallback, SupportedLanguages.Fallback, SupportedLanguages.Fallback);

    /// <summary>A profile whose interface and help follow <paramref name="interfaceCulture"/> while study stays in English.</summary>
    public static LanguageProfile ForInterface(string interfaceCulture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(interfaceCulture);
        return new LanguageProfile(interfaceCulture, SupportedLanguages.Fallback, interfaceCulture);
    }
}
