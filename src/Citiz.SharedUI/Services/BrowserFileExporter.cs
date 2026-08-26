using Microsoft.JSInterop;

namespace Citiz.SharedUI.Services;

/// <summary>Downloads a file through the browser's <c>&lt;a download&gt;</c> mechanism.</summary>
public sealed class BrowserFileExporter(IJSRuntime js) : IFileExporter
{
    /// <inheritdoc />
    public ValueTask ExportAsync(string fileName, string text, string mediaType = "application/json") =>
        js.InvokeVoidAsync("citiz.download", fileName, text, mediaType);
}
