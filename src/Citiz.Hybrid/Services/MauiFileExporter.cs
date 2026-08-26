using System.Text;
using CommunityToolkit.Maui.Storage;
using Citiz.SharedUI.Services;

namespace Citiz.Hybrid.Services;

/// <summary>
/// Offers a file through the platform's native save picker, via CommunityToolkit.Maui.Storage — the
/// Hybrid equivalent of Citiz.Web's <c>&lt;a download&gt;</c>-based <see cref="BrowserFileExporter"/>.
/// Registered on Android/iOS/MacCatalyst; Windows uses <c>Platforms/Windows/WindowsFileExporter.cs</c>
/// instead, because this package's own Windows implementation doesn't work there (see its doc comment).
/// </summary>
public sealed class MauiFileExporter(IFileSaver fileSaver) : IFileExporter
{
    /// <inheritdoc />
    public async ValueTask ExportAsync(string fileName, string text, string mediaType = "application/json")
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));

        // Fire-and-forget, matching IFileExporter's contract: BrowserFileExporter can't report
        // success/failure back either (the browser's own download UI owns that), and the learner
        // closing the native save picker is a normal outcome, not an error to surface.
        await fileSaver.SaveAsync(fileName, stream, CancellationToken.None);
    }
}
