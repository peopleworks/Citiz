using Microsoft.JSInterop;

namespace Citiz.SharedUI.Services;

/// <summary>
/// The thin JavaScript bridge: localStorage, the document's language attributes, the live region
/// for screen readers, and file download. Everything here stays on the device.
/// </summary>
public sealed class BrowserStorage(IJSRuntime js)
{
    /// <summary>Reads a localStorage value, or <c>null</c>.</summary>
    public ValueTask<string?> GetAsync(string key) => js.InvokeAsync<string?>("citiz.storage.get", key);

    /// <summary>Writes a localStorage value.</summary>
    public ValueTask SetAsync(string key, string value) => js.InvokeVoidAsync("citiz.storage.set", key, value);

    /// <summary>Removes a localStorage value.</summary>
    public ValueTask RemoveAsync(string key) => js.InvokeVoidAsync("citiz.storage.remove", key);

    /// <summary>The browser's preferred language tag (<c>navigator.language</c>).</summary>
    public ValueTask<string?> GetBrowserLanguageAsync() => js.InvokeAsync<string?>("citiz.browserLanguage");

    /// <summary>Sets <c>lang</c> and <c>dir</c> on the document element, so the whole page (not just a div) follows the interface language.</summary>
    public ValueTask SetDocumentLanguageAsync(string lang, string dir) => js.InvokeVoidAsync("citiz.setDocumentLanguage", lang, dir);

    /// <summary>Sets (or, for <c>null</c>, clears) <c>data-theme</c> on the document element to force light/dark, overriding the system preference.</summary>
    public ValueTask ApplyThemeAsync(string? theme) => js.InvokeVoidAsync("citiz.setTheme", theme);

    /// <summary>Announces a message to assistive technology through the page's live region.</summary>
    public ValueTask AnnounceAsync(string text) => js.InvokeVoidAsync("citiz.announce", text);
}
