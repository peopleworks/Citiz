using Citiz.SharedUI.Services;
using Microsoft.Windows.Storage.Pickers;

namespace Citiz.Hybrid.Platforms.Windows;

/// <summary>
/// Windows-specific <see cref="IFileExporter"/>, using the Windows App SDK 1.8+ pickers
/// (<c>Microsoft.Windows.Storage.Pickers</c>) instead of CommunityToolkit.Maui.Storage's own Windows
/// <c>FileSaver</c>. That implementation uses the classic WinRT <c>Windows.Storage.Pickers</c> API,
/// which requires package identity and throws a COMException in this unpackaged app (confirmed live)
/// regardless of how correctly its owner window is initialized — a documented limitation, not
/// something fixable by passing a better window handle. The Windows App SDK pickers used here were
/// built specifically to also work for unpackaged desktop apps.
/// </summary>
public sealed class WindowsFileExporter : IFileExporter
{
    /// <inheritdoc />
    public async ValueTask ExportAsync(string fileName, string text, string mediaType = "application/json")
    {
        if (Microsoft.Maui.Controls.Application.Current?.Windows is not [{ Handler.PlatformView: Microsoft.UI.Xaml.Window window }, ..])
        {
            return;
        }

        var picker = new FileSavePicker(window.AppWindow.Id)
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = Path.GetFileNameWithoutExtension(fileName),
        };

        var extension = Path.GetExtension(fileName);
        if (!string.IsNullOrEmpty(extension))
        {
            picker.FileTypeChoices.Add(extension, [extension]);
            picker.DefaultFileExtension = extension;
        }

        // A cancelled picker (result is null) is a normal outcome, not an error — nothing to
        // surface, matching BrowserFileExporter's fire-and-forget contract.
        var result = await picker.PickSaveFileAsync();
        if (result is not null)
        {
            await File.WriteAllTextAsync(result.Path, text);
        }
    }
}
