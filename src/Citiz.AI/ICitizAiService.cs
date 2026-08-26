using Citiz.Core.Exams;

namespace Citiz.AI;

/// <summary>Where a provider runs, which the interface must disclose before using it (Docs/Privacy/LOCAL_VS_CLOUD.md).</summary>
public enum AiExecutionClass
{
    /// <summary>On the device; nothing leaves it.</summary>
    Local,

    /// <summary>A remote service the learner opted into; the interface says what is sent and to whom.</summary>
    OptionalCloud,
}

/// <summary>What a provider is asked to judge: a learner's response to one official question.</summary>
/// <param name="QuestionId">The question's content id.</param>
/// <param name="Prompt">The official question.</param>
/// <param name="AcceptedAnswers">The official accepted answers, already resolved for dynamic questions. The provider may only accept these.</param>
/// <param name="Response">What the learner typed or said (transcribed).</param>
/// <param name="HelpCulture">Language the feedback should be in.</param>
public sealed record AnswerEvaluationRequest(
    string QuestionId,
    string Prompt,
    IReadOnlyList<string> AcceptedAnswers,
    string Response,
    string HelpCulture);

/// <summary>A provider's judgement. Feedback is an interface-translation key, so it renders in the help language and no provider writes prose the learner sees unreviewed.</summary>
/// <param name="Accepted">Whether the response matched an official answer.</param>
/// <param name="MatchedAnswer">The official answer matched or closest, in its official form.</param>
/// <param name="Confidence">0 to 1.</param>
/// <param name="MatchKind">How the deterministic stage classified the response.</param>
/// <param name="FeedbackKey">Translation key of the feedback line.</param>
/// <param name="Provider">Name of the provider that produced this, for the disclosure line.</param>
public sealed record AnswerEvaluation(
    bool Accepted,
    string? MatchedAnswer,
    double Confidence,
    AnswerMatchKind MatchKind,
    string FeedbackKey,
    string Provider);

/// <summary>
/// The abstraction every AI feature goes through. The design allows a provider to converse, explain
/// and evaluate; it forbids it to create or change an official answer, and this interface makes that
/// structural: providers receive the accepted answers and return a judgement about them.
/// </summary>
public interface ICitizAiService
{
    /// <summary>Provider name shown in the interface.</summary>
    string Name { get; }

    /// <summary>Where the provider runs.</summary>
    AiExecutionClass ExecutionClass { get; }

    /// <summary>Judges a learner's response against the official answers.</summary>
    Task<AnswerEvaluation> EvaluateAnswerAsync(AnswerEvaluationRequest request, CancellationToken cancellationToken = default);
}
