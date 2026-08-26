namespace Citiz.SharedUI.Services;

/// <summary>
/// Offers a piece of text as a file the learner keeps, e.g. an exported progress snapshot. A
/// browser has one way to do this (the <c>&lt;a download&gt;</c> trick <see cref="BrowserFileExporter"/>
/// uses); a native host has none of that DOM machinery and needs its own file-save flow, which is
/// why this sits behind an interface separate from <see cref="BrowserStorage"/>.
/// </summary>
public interface IFileExporter
{
    /// <summary>Offers <paramref name="text"/> for download as <paramref name="fileName"/>.</summary>
    ValueTask ExportAsync(string fileName, string text, string mediaType = "application/json");
}
