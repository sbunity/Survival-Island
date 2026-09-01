using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Sudoku Minigame", menuName = "Data/Trader/Minigames/Sudoku")]
    public class SudokuMinigameDefinition : TraderMinigameDefinition
    {
        [BoxGroup("Sudoku Field", "Sudoku Field")]
        [SerializeField] SudokuMinigameView viewPrefab;

        [BoxGroup("Sudoku Field")]
        [SerializeField] Sprite fieldSprite;

        [BoxGroup("Sudoku Field")]
        [SerializeField] Rect gridRect = new(0.0651f, 0.1161f, 0.8672f, 0.8253f);

        [BoxGroup("Sudoku Field")]
        [SerializeField, Range(0.1f, 1f)] float cellScale = 0.78f;

        [BoxGroup("Sudoku Rules", "Sudoku Rules")]
        [SerializeField] CurrencyType[] symbolPool = new[]
        {
            CurrencyType.Wood,
            CurrencyType.Stone,
            CurrencyType.Fiber,
            CurrencyType.Iron,
            CurrencyType.Planks,
            CurrencyType.Bricks,
            CurrencyType.Rope,
            CurrencyType.Berries,
            CurrencyType.Coconut
        };

        [BoxGroup("Sudoku Rules")]
        [SerializeField] SudokuDifficulty[] difficulties = new[] { new SudokuDifficulty() };

        [BoxGroup("Sudoku Reward", "Sudoku Reward")]
        [SerializeField] CurrencyType[] rewardPool;

        [BoxGroup("Sudoku Reward")]
        [SerializeField] DuoInt rewardAmountRange = new(20, 40);

        [BoxGroup("Sudoku Reward")]
        [SerializeField, Min(1)] int rewardAmountStep = 5;

        public override Resource[] RollReward(int seed)
        {
            var pool = rewardPool.IsNullOrEmpty() ? symbolPool : rewardPool;

            if (pool.IsNullOrEmpty())
                return base.RollReward(seed);

            var difficulty = SudokuDifficulty.Pick(difficulties, seed);
            var multiplier = difficulty != null ? difficulty.RewardMultiplier : 1f;

            var currency = pool[Random.Range(0, pool.Length)];
            var amount = Mathf.RoundToInt(rewardAmountRange.Random() * multiplier);

            return new[] { new Resource(currency, SnapAmount(amount, rewardAmountStep)) };
        }

        public override MinigameView CreateView(Transform parent)
        {
            if (viewPrefab == null)
            {
                Debug.LogError($"[Sudoku]: View prefab is not linked on \"{name}\".", this);

                return null;
            }

            var view = Instantiate(viewPrefab, parent);
            view.Configure(BuildSettings());

            return view;
        }

        private SudokuSettings BuildSettings()
        {
            return new SudokuSettings
            {
                FieldSprite = fieldSprite,
                GridRect = gridRect,
                CellScale = cellScale,
                SymbolPool = symbolPool,
                Difficulties = difficulties
            };
        }
    }
}
