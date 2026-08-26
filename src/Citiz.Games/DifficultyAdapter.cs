namespace Citiz.Games;

/// <summary>Maps a learner's mastery of an area to a difficulty level from 1 (easiest) to 4.</summary>
public static class DifficultyAdapter
{
    /// <summary>Highest level.</summary>
    public const int MaxLevel = 4;

    /// <summary>The level for a mastery share between 0 and 1.</summary>
    public static int LevelFor(double mastery)
    {
        if (double.IsNaN(mastery))
        {
            return 1;
        }

        return Math.Clamp(mastery, 0, 1) switch
        {
            < 0.35 => 1,
            < 0.65 => 2,
            < 0.85 => 3,
            _ => MaxLevel,
        };
    }
}
