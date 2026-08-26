using Citiz.AI;
using Citiz.Content;
using Citiz.Discovery;
using Citiz.Hybrid.Services;
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
		// Citiz.SharedUI is identical between the two hosts. Content and translations read from
		// the app package (Services/AppPackageContentStore, .../AppPackageTranslationCatalogLoader)
		// instead of over HTTP — there is no server behind a BlazorWebView to fetch them from.
		// One thing is still a placeholder, tracked in README.md: ISpeechService uses the
		// browser's Web Speech API via JS interop, which may or may not work inside each
		// platform's native WebView and needs device testing.
		builder.Services.AddScoped<BrowserStorage>();
		builder.Services.AddScoped<ISpeechService, WebSpeechService>();
		builder.Services.AddScoped<IFileExporter, BrowserFileExporter>();
		builder.Services.AddScoped<IContentStore, AppPackageContentStore>();
		builder.Services.AddScoped<ContentRepository>();
		builder.Services.AddScoped<ITranslationCatalogLoader, AppPackageTranslationCatalogLoader>();
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
