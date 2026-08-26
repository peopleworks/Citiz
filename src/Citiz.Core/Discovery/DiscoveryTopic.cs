using Citiz.Core.Content;

namespace Citiz.Core.Discovery;

/// <summary>
/// One "Today in the United States" capsule: a short, sourced piece about the country's history,
/// geography, people, institutions, culture, innovation or nature, with the vocabulary it teaches
/// and the exam questions it connects to. Editorial content, never an official answer.
/// </summary>
/// <param name="Id">Stable slug, e.g. <c>grand-canyon</c>.</param>
/// <param name="Category">One of: history, geography, people, institutions, culture, innovation, nature.</param>
/// <param name="Title">Title in English.</param>
/// <param name="Summary">The editorial summary, in plain English.</param>
/// <param name="SimpleEnglish">The same idea in simpler English for beginning learners.</param>
/// <param name="EstimatedMinutes">How long the capsule takes to read.</param>
/// <param name="Difficulty">English level: beginner, intermediate or advanced.</param>
/// <param name="Vocabulary">English words the capsule teaches.</param>
/// <param name="RelatedQuestionIds">Civics question ids this capsule gives context for.</param>
/// <param name="RelatedPlaces">States, cities or landmarks involved.</param>
/// <param name="ReviewStatus">Editorial state.</param>
/// <param name="Sources">Where the facts come from.</param>
public sealed record DiscoveryTopic(
    string Id,
    string Category,
    string Title,
    string Summary,
    string SimpleEnglish,
    int EstimatedMinutes,
    string Difficulty,
    IReadOnlyList<string> Vocabulary,
    IReadOnlyList<string> RelatedQuestionIds,
    IReadOnlyList<string> RelatedPlaces,
    ReviewStatus ReviewStatus,
    IReadOnlyList<SourceReference> Sources);
