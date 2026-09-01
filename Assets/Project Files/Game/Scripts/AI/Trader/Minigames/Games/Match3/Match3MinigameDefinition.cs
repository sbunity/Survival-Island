using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Match3 Minigame", menuName = "Data/Trader/Minigames/Match3")]
    public class Match3MinigameDefinition : TraderMinigameDefinition
    {
        [BoxGroup("Match3 Field", "Match3 Field")]
        [SerializeField] Match3MinigameView viewPrefab;

        [BoxGroup("Match3 Field")]
        [SerializeField] Sprite fieldSprite;

        [BoxGroup("Match3 Field")]
        [SerializeField, Min(3)] int columns = 6;

        [BoxGroup("Match3 Field")]
        [SerializeField, Min(3)] int rows = 6;

        [BoxGroup("Match3 Field")]
        [SerializeField] Rect gridRect = new(0.0651f, 0.1161f, 0.8672f, 0.8253f);

        [BoxGroup("Match3 Field")]
        [SerializeField, Range(0.1f, 1f)] float tileScale = 0.78f;

        [BoxGroup("Match3 Rules", "Match3 Rules")]
        [SerializeField] CurrencyType[] tilePool = new[]
        {
            CurrencyType.Wood,
            CurrencyType.Stone,
            CurrencyType.Fiber,
            CurrencyType.Iron,
            CurrencyType.Planks,
            CurrencyType.Bricks,
            CurrencyType.Rope,
            CurrencyType.Berries,
            CurrencyType.Coconut,
            CurrencyType.Pumpkin,
            CurrencyType.Fish
        };

        [BoxGroup("Match3 Rules")]
        [SerializeField, Min(3)] int tileTypesPerGame = 6;

        [BoxGroup("Match3 Rules")]
        [SerializeField] DuoInt movesRange = new(18, 26);

        [BoxGroup("Match3 Rules")]
        [SerializeField] DuoInt goalAmountRange = new(15, 25);

        public override Resource[] RollReward(int seed)
        {
            if (tilePool.IsNullOrEmpty())
                return base.RollReward(seed);

            var currency = tilePool[Random.Range(0, tilePool.Length)];

            return new[] { new Resource(currency, Mathf.Max(1, goalAmountRange.Random())) };
        }

        public override MinigameView CreateView(Transform parent)
        {
            if (viewPrefab == null)
            {
                Debug.LogError($"[Match3]: View prefab is not linked on \"{name}\".", this);

                return null;
            }

            var view = Instantiate(viewPrefab, parent);
            view.Configure(BuildSettings());

            return view;
        }

        private Match3Settings BuildSettings()
        {
            return new Match3Settings
            {
                FieldSprite = fieldSprite,
                Columns = columns,
                Rows = rows,
                GridRect = gridRect,
                TileScale = tileScale,
                TilePool = tilePool,
                TileTypesPerGame = Mathf.Min(tileTypesPerGame, tilePool != null ? tilePool.Length : tileTypesPerGame),
                MovesRange = movesRange
            };
        }
    }
}
