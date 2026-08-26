namespace Citiz.Localization.Tests;

public sealed class LanguageTests
{
    [Theory]
    [InlineData("es", "es")]
    [InlineData("es-MX", "es")]
    [InlineData("ES-419", "es")]
    [InlineData("zh", "zh-Hans")]
    [InlineData("zh-CN", "zh-Hans")]
    [InlineData("zh-Hans-CN", "zh-Hans")]
    [InlineData("zh-TW", "zh-Hant")]
    [InlineData("zh-HK", "zh-Hant")]
    [InlineData("zh-Hant", "zh-Hant")]
    [InlineData("tl", "fil")]
    [InlineData("fil-PH", "fil")]
    [InlineData("vi-VN", "vi")]
    [InlineData("ar-EG", "ar")]
    [InlineData("fr", "en")]
    [InlineData("", "en")]
    [InlineData(null, "en")]
    public void Browser_language_maps_to_a_supported_pack(string? browser, string expected) =>
        Assert.Equal(expected, SupportedLanguages.NormalizeBrowserLanguage(browser));

    [Fact]
    public void Interface_study_and_help_languages_are_independent()
    {
        var profile = new LanguageProfile("es", "en", "vi");

        Assert.NotEqual(profile.InterfaceCulture, profile.StudyCulture);
        Assert.NotEqual(profile.InterfaceCulture, profile.HelpCulture);
        Assert.Equal("en", LanguageProfile.ForInterface("ar").StudyCulture);
        Assert.Equal("ar", LanguageProfile.ForInterface("ar").HelpCulture);
    }

    [Fact]
    public void Arabic_is_right_to_left_and_the_rest_are_not()
    {
        Assert.Equal("rtl", SupportedLanguages.Find("ar")!.HtmlDirection);
        Assert.All(SupportedLanguages.All.Where(l => l.Code != "ar"), l => Assert.Equal("ltr", l.HtmlDirection));
    }

    [Fact]
    public void English_is_the_source_and_every_other_pack_declares_its_review_state()
    {
        Assert.Equal(TranslationReviewStatus.Source, SupportedLanguages.Find("en")!.Status);
        Assert.All(SupportedLanguages.All.Where(l => l.Code != "en"), l => Assert.NotEqual(TranslationReviewStatus.Source, l.Status));
    }

    [Fact]
    public async Task Service_falls_back_to_english_then_to_the_bracketed_key()
    {
        var loader = new StubLoader(new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new() { ["a"] = "A", ["b"] = "B", ["n"] = "{0} items" },
            ["es"] = new() { ["a"] = "Á", ["n"] = "{0} elementos" },
        });
        var service = new LocalizationService(loader);
        var changes = 0;
        service.Changed += () => changes++;

        await service.InitializeAsync(LanguageProfile.ForInterface("es"));

        Assert.Equal("Á", service.T("a"));
        Assert.Equal("B", service.T("b"));
        Assert.Equal("[c]", service.T("c"));
        Assert.Equal("3 elementos", service.T("n", 3));
        Assert.Equal("rtl", SupportedLanguages.Find("ar")!.HtmlDirection);
        Assert.Equal(1, changes);

        await service.SetInterfaceCultureAsync("xx");

        Assert.Equal("en", service.Profile.InterfaceCulture);
        Assert.Equal(2, changes);
    }

    private sealed class StubLoader(Dictionary<string, Dictionary<string, string>> packs) : ITranslationCatalogLoader
    {
        public Task<TranslationCatalog?> LoadAsync(string culture, CancellationToken cancellationToken = default) =>
            Task.FromResult(packs.TryGetValue(culture, out var values) ? new TranslationCatalog(culture, values) : null);
    }
}
