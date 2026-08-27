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
        public float WinMultiplier => winMultiplier;

        public abstract MinigameView CreateView(Transform parent);

        public Resource RollStake()
        {
            if (stakeType != MinigameStakeType.Wager || stakeCurrencies.IsNullOrEmpty())
                return default;

            var minAmount = Mathf.Min(stakeAmountRange.firstValue, stakeAmountRange.secondValue);

            var affordable = new List<CurrencyType>();
            for (var i = 0; i < stakeCurrencies.Length; i++)
            {
                if (CurrencyController.HasAmount(stakeCurrencies[i], minAmount))
                    affordable.Add(stakeCurrencies[i]);
            }

            var currency = affordable.Count > 0
                ? affordable[Random.Range(0, affordable.Count)]
                : stakeCurrencies[Random.Range(0, stakeCurrencies.Length)];

            return new Resource(currency, SnapAmount(stakeAmountRange.Random()));
        }

        private int SnapAmount(int amount)
        {
            if (stakeAmountStep <= 1)
                return Mathf.Max(1, amount);

            return Mathf.Max(stakeAmountStep, Mathf.RoundToInt(amount / (float)stakeAmountStep) * stakeAmountStep);
        }

        #region Editor
        protected bool IsRewardStake() => stakeType == MinigameStakeType.Reward;
        protected bool IsWagerStake() => stakeType == MinigameStakeType.Wager;
        #endregion
    }
}
