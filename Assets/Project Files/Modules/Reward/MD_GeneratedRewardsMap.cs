// Auto-generated file. Do not edit.
// Regenerate via: Tools/Rewards/Regenerate Map
using UnityEngine;

namespace Watermelon
{
    static class GeneratedRewardsMap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            RewardsMap.Register(typeof(Watermelon.CurrencyReward), typeof(Watermelon.CurrencyRewardView));
            RewardsMap.Register(typeof(Watermelon.NoAdsReward), typeof(Watermelon.NoAdsRewardView));
            RewardsMap.Register(typeof(Watermelon.SkinReward), typeof(Watermelon.SkinRewardView));
        }
    }
}