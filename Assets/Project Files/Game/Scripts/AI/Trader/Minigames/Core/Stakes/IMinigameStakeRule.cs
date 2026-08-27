namespace Watermelon
{
    public interface IMinigameStakeRule
    {
        MinigameStakeType Type { get; }

        Resource Stake { get; }

        Resource[] Prize { get; }

        bool CanStart(out string blockReason);

        void Charge();

        void Payout(MinigameResult result);
    }
}
