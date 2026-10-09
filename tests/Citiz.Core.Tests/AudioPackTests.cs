using Citiz.Core.Audio;
using Citiz.Core.Content;

namespace Citiz.Core.Tests;

public sealed class AudioPackTests
{
    private const string Sha = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    private static AudioPack Pack(params AudioClip[] clips) => new(
        "citiz-voice-capsules", AudioPackKind.Synthetic, "T", "D", null, 1, new Uri("https://audio.example/p/v1/"),
        clips.Sum(c => c.Bytes), "L", "ElevenLabs · Test", null, ReviewStatus.Approved, [TestData.Source], clips);

    private static AudioClip Capsule(string topicId, string variant) =>
        new($"c-{topicId}-{variant}", AudioClipRole.Capsule, $"c-{topicId}-{variant}.mp3", 10, 3, Sha, null, null, null, topicId, variant, Sha);

    [Fact]
    public void Finds_the_capsule_clip_for_the_text_on_screen()
    {
        var pack = Pack(Capsule("washington-dc", AudioClip.SimpleVariant), Capsule("washington-dc", AudioClip.FullVariant), Capsule("statue-of-liberty", AudioClip.SimpleVariant));

        Assert.Equal("c-washington-dc-simple", pack.CapsuleFor("washington-dc", simple: true)?.Id);
        Assert.Equal("c-washington-dc-full", pack.CapsuleFor("washington-dc", simple: false)?.Id);
        Assert.Null(pack.CapsuleFor("statue-of-liberty", simple: false));
        Assert.Null(pack.CapsuleFor("grand-canyon", simple: true));
        Assert.Null(pack.WordFor("washington-dc"));
    }

    [Theory]
    // Expected values from Python's hashlib.sha256(text.encode("utf-8")).hexdigest(), which the generator uses.
    [InlineData("E", "a9f51566bd6705f7ea6ad54bb9deb449f795582d6529a0e22207b8981233ec58")]
    [InlineData("Café — “quoted”", "85b9c514c4f7581b578816ed6c50a73c0ffaaed9f9828444a45e064ae4340d8a")]
    public void Text_digest_matches_the_generator(string text, string expected) =>
        Assert.Equal(expected, AudioClip.TextDigest(text));
}
