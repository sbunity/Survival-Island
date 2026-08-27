namespace Watermelon
{
    public class FixedRewardStakeRule : IMinigameStakeRule
    {
        public const string ANALYTICS_SOURCE = "trader_minigame";

        private readonly Resource[] prize;

        public MinigameStakeType Type => MinigameStakeType.Reward;
        public Resource Stake => default;
        public Resource[] Prize => prize;

        public FixedRewardStakeRule(Resource[] prize)
        {
            this.prize = prize ?? new Resource[0];
        }

        public bool CanStart(out string blockReason)
        {
            blockReason = null;

            return true;
        }

        public void Charge() { }

        public void Payout(MinigameResult result)
        {
            if (!result.IsWin)
                return;

            for (var i = 0; i < prize.Length; i++)
                CurrencyController.Add(prize[i].currency, prize[i].amount, ANALYTICS_SOURCE);
        }
    }
}
