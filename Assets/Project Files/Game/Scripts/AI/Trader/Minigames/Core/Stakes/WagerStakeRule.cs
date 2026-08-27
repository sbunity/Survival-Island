using UnityEngine;

namespace Watermelon
{
    public class WagerStakeRule : IMinigameStakeRule
    {
        private readonly Resource stake;
        private readonly Resource[] prize;

        public MinigameStakeType Type => MinigameStakeType.Wager;
        public Resource Stake => stake;
        public Resource[] Prize => prize;

        public WagerStakeRule(Resource stake, float winMultiplier)
        {
            this.stake = stake;

            var payout = Mathf.Max(stake.amount, Mathf.RoundToInt(stake.amount * winMultiplier));

            prize = new Resource[] { new Resource(stake.currency, payout) };
        }

        public bool CanStart(out string blockReason)
        {
            if (stake.amount <= 0)
            {
                blockReason = "Stake is not set";

                return false;
            }

            if (!CurrencyController.HasAmount(stake.currency, stake.amount))
            {
                blockReason = "Not enough resources";

                return false;
            }

            blockReason = null;

            return true;
        }

        public void Charge()
        {
            CurrencyController.Substract(stake.currency, stake.amount, FixedRewardStakeRule.ANALYTICS_SOURCE);
        }

        public void Payout(MinigameResult result)
        {
            if (!result.IsWin)
                return;

            for (var i = 0; i < prize.Length; i++)
                CurrencyController.Add(prize[i].currency, prize[i].amount, FixedRewardStakeRule.ANALYTICS_SOURCE);
        }
    }
}
