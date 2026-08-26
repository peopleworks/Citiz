using Citiz.Localization;
using Microsoft.AspNetCore.Components;

namespace Citiz.SharedUI.Components;

/// <summary>
/// Base for every page and component that renders interface strings. Blazor skips re-rendering a
/// child whose parameters did not change, so a language switch would leave pages and badges in the
/// old language; listening to <see cref="LocalizationService.Changed"/> here makes every string
/// follow the switch without a page reload.
/// </summary>
public abstract class LocalizedComponentBase : ComponentBase, IDisposable
{
    /// <summary>The interface translations.</summary>
    [Inject]
    protected LocalizationService L { get; set; } = default!;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        L.Changed += OnLanguageChanged;
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        L.Changed -= OnLanguageChanged;
        GC.SuppressFinalize(this);
    }

    private void OnLanguageChanged() => InvokeAsync(StateHasChanged);
}
