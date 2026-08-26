using Citiz.AI;
using Citiz.Content;
using Citiz.Discovery;
using Citiz.Localization;
using Citiz.SharedUI;
using Citiz.SharedUI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

// Citiz.Web — the browser client. There is no server behind it: content and translations are
// static files next to the app, progress lives in localStorage, and the answer evaluator is the
// deterministic matcher compiled into the WebAssembly bundle.

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<BrowserStorage>();
builder.Services.AddScoped<ISpeechService, WebSpeechService>();
builder.Services.AddScoped<IFileExporter, BrowserFileExporter>();
builder.Services.AddScoped<IContentStore, HttpContentStore>();
builder.Services.AddScoped<ContentRepository>();
builder.Services.AddScoped<ITranslationCatalogLoader, HttpTranslationCatalogLoader>();
builder.Services.AddScoped<LocalizationService>();
builder.Services.AddScoped<LearnerState>();
builder.Services.AddScoped<StudyService>();
builder.Services.AddScoped<DiscoveryEngine>();
builder.Services.AddScoped<ICitizAiService, NoAiFallbackService>();

await builder.Build().RunAsync();
