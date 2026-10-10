/// <summary>
/// Stores lives for the current run. Static state survives stage reloads,
/// and MainMenuManager resets it when a new run begins.
/// </summary>
public static class PlayerLifeSystem
{
    public const int StartingLives = 3;

    public static int RemainingLives { get; private set; } = StartingLives;

    public static void ResetForNewGame()
    {
        RemainingLives = StartingLives;
    }

    public static int LoseLife()
    {
        RemainingLives = UnityEngine.Mathf.Max(0, RemainingLives - 1);
        return RemainingLives;
    }
}
