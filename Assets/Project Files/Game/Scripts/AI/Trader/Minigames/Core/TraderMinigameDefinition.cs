using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public abstract class TraderMinigameDefinition : ScriptableObject
    {
        [UniqueID, Order(-2)]
        [SerializeField] string id;
        public string ID => id;

        [BoxGroup("Info", "Info")]
        [SerializeField] string title;
        public string Title => title;

        [BoxGroup("Info")]
        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        [BoxGroup("Info")]
        [SerializeField, TextArea(2, 4)] string description;
        public string Description => description;

        [BoxGroup("Info")]
        [SerializeField] Sprite background;
        public Sprite Background => background;

        [BoxGroup("Info")]
        [SerializeField] MinigameStageType stage = MinigameStageType.None;
        public MinigameStageType Stage => stage;

        [BoxGroup("Selection", "Selection")]
        [SerializeField, Min(0f)] float weight = 1f;
        public float Weight => weight;

        [BoxGroup("Stake", "Stake & Prize")]
        [SerializeField] MinigameStakeType stakeType;
        public MinigameStakeType StakeType => stakeType;

        [BoxGroup("Stake")]
        [SerializeField, ShowIf("IsRewardStake")] Resource[] reward;
        public Resource[] Reward => reward;

        [BoxGroup("Stake")]
        [SerializeField, ShowIf("IsWagerStake")] CurrencyType[] stakeCurrencies;

        [BoxGroup("Stake")]
        [SerializeField, ShowIf("IsWagerStake")] DuoInt stakeAmountRange = new DuoInt(10, 30);

        [BoxGroup("Stake")]
        [SerializeField, ShowIf("IsWagerStake"), Min(1)] int stakeAmountStep = 5;

        [BoxGroup("Stake")]
        [SerializeField, ShowIf("IsWagerStake"), Min(1f)] float winMultiplier = 2f;

        public abstract MinigameView CreateView(Transform parent);

        public virtual bool IsAvailable()
        {
            return stakeType == MinigameStakeType.Wager
                ? ResourceUnlocks.HasAnyUnlocked(stakeCurrencies)
                : ResourceUnlocks.AreAllUnlocked(reward);
        }

        public virtual Resource[] RollReward(int seed)
        {
            return reward;
        }

        public virtual float RollWinMultiplier(int seed)
        {
            return winMultiplier;
        }

        public Resource RollStake()
        {
            if (stakeType != MinigameStakeType.Wager || stakeCurrencies.IsNullOrEmpty())
                return default;

            var available = new List<CurrencyType>();

            if (ResourceUnlocks.FilterUnlocked(stakeCurrencies, available) == 0)
                return default;

            var minAmount = Mathf.Min(stakeAmountRange.firstValue, stakeAmountRange.secondValue);

            var affordable = new List<CurrencyType>();
            for (var i = 0; i < available.Count; i++)
            {
                if (CurrencyController.HasAmount(available[i], minAmount))
                    affordable.Add(available[i]);
            }

            var currency = affordable.Count > 0
                ? affordable[Random.Range(0, affordable.Count)]
                : available[Random.Range(0, available.Count)];

            return new Resource(currency, SnapAmount(stakeAmountRange.Random(), stakeAmountStep));
        }

        protected bool IsRewardPoolAvailable(CurrencyType[] pool)
        {
            return pool.IsNullOrEmpty()
                ? ResourceUnlocks.AreAllUnlocked(reward)
                : ResourceUnlocks.HasAnyUnlocked(pool);
        }

        protected static int SnapAmount(int amount, int step)
        {
            if (step <= 1)
                return Mathf.Max(1, amount);

            return Mathf.Max(step, Mathf.RoundToInt(amount / (float)step) * step);
        }

        #region Editor
        protected bool IsRewardStake() => stakeType == MinigameStakeType.Reward;
        protected bool IsWagerStake() => stakeType == MinigameStakeType.Wager;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = UniqueIDUtils.GetUniqueID();

                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
        #endregion
    }
}
