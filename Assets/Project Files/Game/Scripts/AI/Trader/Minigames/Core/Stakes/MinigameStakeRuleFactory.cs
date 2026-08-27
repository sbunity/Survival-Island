using UnityEngine;

namespace Watermelon
{
    public static class MinigameStakeRuleFactory
    {
        public static IMinigameStakeRule Create(TraderMinigameDefinition definition, Resource rolledStake)
        {
            if (definition == null)
                return null;

            switch (definition.StakeType)
            {
                case MinigameStakeType.Reward:
                    return new FixedRewardStakeRule(definition.Reward);

                case MinigameStakeType.Wager:
                    return new WagerStakeRule(rolledStake, definition.WinMultiplier);

                default:
                    Debug.LogError($"[Trader Minigames]: Unhandled stake type {definition.StakeType} on \"{definition.name}\".", definition);

                    return null;
            }
        }
    }
}
