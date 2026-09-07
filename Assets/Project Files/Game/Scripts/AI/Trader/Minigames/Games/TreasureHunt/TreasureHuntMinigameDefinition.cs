using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Treasure Hunt Minigame", menuName = "Data/Trader/Minigames/Treasure Hunt")]
    public class TreasureHuntMinigameDefinition : TraderMinigameDefinition
    {
        [BoxGroup("Treasure Field", "Treasure Field")]
        [SerializeField] TreasureHuntMinigameView viewPrefab;

        [BoxGroup("Treasure Field")]
        [SerializeField] Sprite fieldSprite;

        [BoxGroup("Treasure Field")]
        [SerializeField] Sprite buriedSprite;

        [BoxGroup("Treasure Field")]
        [SerializeField] Sprite dugSprite;

        [BoxGroup("Treasure Field")]
        [SerializeField] Rect gridRect = new(0.0814f, 0.0738f, 0.8443f, 0.8368f);

        [BoxGroup("Treasure Field")]
        [SerializeField, Range(0.1f, 1.5f)] float cellScale = 1f;

        [BoxGroup("Treasure Field")]
        [SerializeField, Range(0.1f, 1.5f)] float prizeScale = 0.6f;

        [BoxGroup("Treasure Rules", "Treasure Rules")]
        [SerializeField] TreasureDistanceMode distanceMode = TreasureDistanceMode.Chebyshev;

        [BoxGroup("Treasure Rules")]
        [Tooltip("Ordered from the hottest to the coldest. The last band catches every remaining distance.")]
        [SerializeField] TreasureHintBand[] hintBands = new[]
        {
            new TreasureHintBand("Hot", 1, new Color(0.85f, 0.27f, 0.15f)),
            new TreasureHintBand("Warm", 2, new Color(0.92f, 0.6f, 0.13f)),
            new TreasureHintBand("Cold", int.MaxValue, new Color(0.2f, 0.48f, 0.8f))
        };

        [BoxGroup("Treasure Rules")]
        [SerializeField] TreasureHuntDifficulty[] difficulties = new[] { new TreasureHuntDifficulty() };

        [BoxGroup("Treasure Reward", "Treasure Reward")]
        [SerializeField] CurrencyType[] rewardPool;

        [BoxGroup("Treasure Reward")]
        [SerializeField] DuoInt rewardAmountRange = new(20, 40);

        [BoxGroup("Treasure Reward")]
        [SerializeField, Min(1)] int rewardAmountStep = 5;

        public override Resource[] RollReward(int seed)
        {
            if (rewardPool.IsNullOrEmpty())
                return base.RollReward(seed);

            var difficulty = MinigameDifficultyPicker.Pick(difficulties, seed);
            var multiplier = difficulty != null ? difficulty.RewardMultiplier : 1f;

            var currency = rewardPool[Random.Range(0, rewardPool.Length)];
            var amount = Mathf.RoundToInt(rewardAmountRange.Random() * multiplier);

            return new[] { new Resource(currency, SnapAmount(amount, rewardAmountStep)) };
        }

        public override MinigameView CreateView(Transform parent)
        {
            if (viewPrefab == null)
            {
                Debug.LogError($"[Treasure Hunt]: View prefab is not linked on \"{name}\".", this);

                return null;
            }

            var view = Instantiate(viewPrefab, parent);
            view.Configure(BuildSettings());

            return view;
        }

        private TreasureHuntSettings BuildSettings()
        {
            return new TreasureHuntSettings
            {
                FieldSprite = fieldSprite,
                BuriedSprite = buriedSprite,
                DugSprite = dugSprite,
                GridRect = gridRect,
                CellScale = cellScale,
                PrizeScale = prizeScale,
                DistanceMode = distanceMode,
                HintBands = hintBands,
                Difficulties = difficulties
            };
        }
    }
}
