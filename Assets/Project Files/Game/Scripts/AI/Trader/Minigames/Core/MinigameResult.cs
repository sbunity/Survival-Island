namespace Watermelon
{
    public readonly struct MinigameResult
    {
        public readonly MinigameOutcome Outcome;

        public readonly float Score;

        public bool IsWin => Outcome == MinigameOutcome.Win;

        public MinigameResult(MinigameOutcome outcome, float score)
        {
            Outcome = outcome;
            Score = score;
        }

        public static MinigameResult Win(float score = 1f) => new(MinigameOutcome.Win, score);
        public static MinigameResult Lose(float score = 0f) => new(MinigameOutcome.Lose, score);
        public static MinigameResult Abandoned => new(MinigameOutcome.Abandoned, 0f);
    }

    public delegate void MinigameFinishedCallback(MinigameResult result);
}
