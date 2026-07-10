namespace AgavePuzzle.UI
{
    public abstract class LevelEndResult
    {
        public abstract string HeaderText { get; }
        public abstract string ButtonLabel { get; }
    }

    public class LevelWonResult : LevelEndResult
    {
        public override string HeaderText => "Level Complete!";
        public override string ButtonLabel => "Play Next";
    }

    public class LevelLostResult : LevelEndResult
    {
        public override string HeaderText => "Out of Moves";
        public override string ButtonLabel => "Try Again";
    }
}