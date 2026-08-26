using Citiz.AI;
using Citiz.Content;
using Citiz.Discovery;
using Citiz.Hybrid.Services;
using Citiz.Localization;
using Citiz.SharedUI.Services;
using CommunityToolkit.Maui;
#if !WINDOWS
using CommunityToolkit.Maui.Storage;
#endif
using Microsoft.Extensions.Logging;

namespace Citiz.Hybrid;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
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

		// CommunityToolkit.Maui.Storage's own Windows FileSaver throws a COMException there (it
		// initializes its picker with the unreliable Process.MainWindowHandle) — confirmed live, see
		// Platforms/Windows/WindowsFileExporter.cs for the real fix and why it isn't used everywhere.
#if WINDOWS
		builder.Services.AddScoped<IFileExporter, Citiz.Hybrid.Platforms.Windows.WindowsFileExporter>();
#else
		builder.Services.AddSingleton(FileSaver.Default);
		builder.Services.AddScoped<IFileExporter, MauiFileExporter>();
#endif
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
