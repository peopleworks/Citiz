using Citiz.AI;
using Citiz.Content;
using Citiz.Discovery;
using Citiz.Localization;
using Citiz.SharedUI.Services;
using Microsoft.Extensions.Logging;

namespace Citiz.Hybrid;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// Same DI shape as Citiz.Web (Citiz.Web/Program.cs), because the whole UI in
		// Citiz.SharedUI is identical between the two hosts. Two things are placeholders, not
		// yet correct for a native host, tracked in README.md:
		//   - IContentStore / ITranslationCatalogLoader still fetch over HttpClient, which has
		//     nothing to fetch from inside a BlazorWebView (no content/i18n files are bundled
		//     into this host's package yet) — content and language loading are expected to fail
		//     until file-system-backed implementations replace these two registrations.
		//   - ISpeechService uses the browser's Web Speech API via JS interop; this may or may
		//     not work inside each platform's native WebView and needs device testing.
		builder.Services.AddScoped(_ => new HttpClient());
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

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
