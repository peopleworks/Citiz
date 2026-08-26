namespace Citiz.Games;

/// <summary>Whether a game can be played in this build.</summary>
public enum GameStatus
{
    /// <summary>Implemented and playable.</summary>
    Playable,

    /// <summary>Designed (see the design document) but not implemented yet. The interface says so; it does not show a dead button.</summary>
    InDesign,
}

/// <summary>A game in the catalog. Titles and descriptions are interface-translation keys so the catalog itself is language-neutral.</summary>
/// <param name="Id">Stable slug.</param>
/// <param name="TitleKey">Translation key of the title.</param>
/// <param name="DescriptionKey">Translation key of the one-line description.</param>
/// <param name="Pillar">Which pillar the game serves: <c>prepare</c>, <c>communicate</c> or <c>discover</c>.</param>
/// <param name="Minutes">Typical session length.</param>
/// <param name="Status">Whether it can be played.</param>
public sealed record GameDefinition(string Id, string TitleKey, string DescriptionKey, string Pillar, int Minutes, GameStatus Status);

/// <summary>The games the design describes, with their current status.</summary>
public static class GameCatalog
{
    /// <summary>Every game, playable ones first.</summary>
    public static IReadOnlyList<GameDefinition> All { get; } =
    [
        new("civics-challenge", "games.civicsChallenge.title", "games.civicsChallenge.description", "prepare", 5, GameStatus.Playable),
        new("lightning-map", "games.lightningMap.title", "games.lightningMap.description", "discover", 3, GameStatus.InDesign),
        new("who-am-i", "games.whoAmI.title", "games.whoAmI.description", "discover", 5, GameStatus.InDesign),
        new("right-word", "games.rightWord.title", "games.rightWord.description", "communicate", 3, GameStatus.InDesign),
        new("order-the-story", "games.orderTheStory.title", "games.orderTheStory.description", "discover", 5, GameStatus.InDesign),
        new("listen-and-find", "games.listenAndFind.title", "games.listenAndFind.description", "communicate", 3, GameStatus.InDesign),
    ];

    /// <summary>Finds a game by id, or <c>null</c>.</summary>
    public static GameDefinition? Find(string id) => All.FirstOrDefault(g => string.Equals(g.Id, id, StringComparison.OrdinalIgnoreCase));
}
