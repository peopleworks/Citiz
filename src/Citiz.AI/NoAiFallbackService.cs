using Citiz.Core.Exams;

namespace Citiz.AI;

/// <summary>
/// The provider that is always available: the deterministic <see cref="AnswerMatcher"/> and nothing
/// else. It is the first stage of every evaluation, and the whole of it when no model is configured,
/// which is the default. Runs locally, sends nothing anywhere.
/// </summary>
public sealed class NoAiFallbackService : ICitizAiService
{
    /// <inheritdoc />
    public string Name => "Deterministic matcher (no AI)";

    /// <inheritdoc />
    public AiExecutionClass ExecutionClass => AiExecutionClass.Local;

    /// <inheritdoc />
    public Task<AnswerEvaluation> EvaluateAnswerAsync(AnswerEvaluationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var match = AnswerMatcher.Evaluate(request.Response, request.AcceptedAnswers);
        return Task.FromResult(new AnswerEvaluation(
            match.IsAccepted,
            match.MatchedAnswer,
            match.Confidence,
            match.Kind,
            FeedbackKeyFor(match.Kind),
            Name));
    }

    /// <summary>The translation key for a match kind.</summary>
    public static string FeedbackKeyFor(AnswerMatchKind kind) => kind switch
    {
        AnswerMatchKind.Exact => "feedback.exact",
        AnswerMatchKind.Contains => "feedback.contains",
        AnswerMatchKind.Close => "feedback.close",
        _ => "feedback.none",
    };
}
