using UnityEngine;

namespace Watermelon
{
    public static class MinigameStakeRuleFactory
    {
        public static IMinigameStakeRule Create(TraderMinigameDefinition definition, int seed, Resource rolledStake, Resource[] rolledReward)
        {
            if (definition == null)
                return null;

            switch (definition.StakeType)
            {
                case MinigameStakeType.Reward:
                    return new FixedRewardStakeRule(rolledReward.IsNullOrEmpty() ? definition.Reward : rolledReward);

                case MinigameStakeType.Wager:
                    return new WagerStakeRule(rolledStake, definition.RollWinMultiplier(seed));

                default:
                    Debug.LogError($"[Trader Minigames]: Unhandled stake type {definition.StakeType} on \"{definition.name}\".", definition);

                    return null;
            }
        }
    }
}
