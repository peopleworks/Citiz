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
		builder.Services.AddScoped<BrowserStorage>();

		// Speech: Windows' WebView2 speaks through the Web Speech API with on-device voices
		// (verified live, README.md). Android's system WebView has no speech voices at all, and
		// WKWebView's are not something to rely on, so the mobile hosts use the platform engine
		// through MAUI's TextToSpeech (Services/MauiTextToSpeechService.cs).
#if WINDOWS
		builder.Services.AddScoped<ISpeechService, WebSpeechService>();
#else
		builder.Services.AddSingleton(TextToSpeech.Default);
		builder.Services.AddScoped<ISpeechService, MauiTextToSpeechService>();
#endif

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
