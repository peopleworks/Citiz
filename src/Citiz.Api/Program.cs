using System.Text.Json.Serialization;
using Citiz.AI;
using Citiz.Content;
using Citiz.Core.English;
using Citiz.Core.Exams;
using Citiz.Discovery;

// Citiz.Api — the optional server. It exposes the same content the browser client loads as static
// files, for integrations and organizations that want one shared instance. It holds no learner data
// and needs no database; the content folder is its only input.

var builder = WebApplication.CreateBuilder(args);

var contentRoot = builder.Configuration["Content:Root"]
    ?? FileContentStore.LocateContentRoot(builder.Environment.ContentRootPath)
    ?? throw new InvalidOperationException("Set Content:Root to the content folder, or run from inside the repository.");

builder.Services.AddSingleton<IContentStore>(new FileContentStore(contentRoot));
builder.Services.AddSingleton<ContentRepository>();
builder.Services.AddSingleton<DiscoveryEngine>();
builder.Services.AddSingleton<ICitizAiService, NoAiFallbackService>();
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.KebabCaseLower)));

var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    if (origins.Length > 0)
    {
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    }
    else if (builder.Environment.IsDevelopment())
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    }
}));

var app = builder.Build();

app.UseCors();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var api = app.MapGroup("/api");

api.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Citiz.Api", contentRoot, utc = DateTimeOffset.UtcNow }));

api.MapGet("/exams/versions", async (ContentRepository content, CancellationToken ct) =>
    Results.Ok(await content.GetExamVersionsAsync(ct)));

api.MapGet("/exams/resolve", async (DateOnly filingDate, ContentRepository content, CancellationToken ct) =>
{
    var version = ExamPolicy.Resolve(filingDate, await content.GetExamVersionsAsync(ct));
    return version is null
        ? Results.Problem(statusCode: 404, title: "No exam version applies", detail: $"No civics-test version covers an N-400 filed on {filingDate:yyyy-MM-dd}.")
        : Results.Ok(version);
});

api.MapGet("/exams/{versionId}/questions", async (string versionId, ContentRepository content, CancellationToken ct) =>
{
    try
    {
        var bank = await content.GetQuestionBankAsync(versionId, ct);
        return Results.Ok(new { bank.VersionId, bank.ReviewStatus, bank.Sources, bank.Questions });
    }
    catch (FileNotFoundException)
    {
        return Results.NotFound();
    }
});

api.MapGet("/exams/dynamic-answers", async (ContentRepository content, CancellationToken ct) =>
    Results.Ok((await content.GetDynamicAnswersAsync(ct)).Values));

api.MapGet("/english/vocabulary/{kind}", async (string kind, ContentRepository content, CancellationToken ct) =>
    Enum.TryParse<VocabularyKind>(kind, ignoreCase: true, out var parsed)
        ? Results.Ok(await content.GetVocabularyAsync(parsed, ct))
        : Results.NotFound());

api.MapGet("/discovery/topics", async (ContentRepository content, CancellationToken ct) =>
    Results.Ok(await content.GetDiscoveryTopicsAsync(ct)));

api.MapGet("/discovery/today", async (ContentRepository content, DiscoveryEngine discovery, CancellationToken ct) =>
{
    var topic = discovery.SelectDaily(await content.GetDiscoveryTopicsAsync(ct), DateOnly.FromDateTime(DateTime.UtcNow));
    return topic is null ? Results.NoContent() : Results.Ok(topic);
});

api.MapPost("/ai/evaluate", async (AnswerEvaluationRequest request, ICitizAiService ai, CancellationToken ct) =>
    Results.Ok(await ai.EvaluateAnswerAsync(request, ct)));

app.Run();

/// <summary>Entry point marker for integration tests.</summary>
public partial class Program;
