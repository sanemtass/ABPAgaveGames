namespace AgavePuzzle.Gameplay
{
    /// Carries the current level index across scene reloads.
    /// Static because scene objects are destroyed on LoadScene.
    public static class LevelProgress
    {
        public static int CurrentLevelIndex { get; set; }
    }
}