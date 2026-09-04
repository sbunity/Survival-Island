using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Shell Game Minigame", menuName = "Data/Trader/Minigames/Shell Game")]
    public class ShellMinigameDefinition : TraderMinigameDefinition
    {
        [BoxGroup("Shell Table", "Shell Table")]
        [SerializeField] ShellMinigameView viewPrefab;

        [BoxGroup("Shell Table")]
        [SerializeField] Sprite tableSprite;

        [BoxGroup("Shell Table")]
        [SerializeField] Sprite shellSprite;

        [BoxGroup("Shell Table")]
        [SerializeField] Rect slotsRect = new(0.1f, 0.28f, 0.8f, 0.34f);

        [BoxGroup("Shell Table")]
        [SerializeField, Range(0.1f, 2f)] float shellScale = 0.92f;

        [BoxGroup("Shell Table")]
        [SerializeField, Range(0.1f, 2f)] float prizeScale = 0.5f;

        [BoxGroup("Shell Rules", "Shell Rules")]
        [SerializeField] ShellDifficulty[] difficulties = new[] { new ShellDifficulty() };

        public override float RollWinMultiplier(int seed)
        {
            var difficulty = MinigameDifficultyPicker.Pick(difficulties, seed);

            return base.RollWinMultiplier(seed) * (difficulty != null ? difficulty.RewardMultiplier : 1f);
        }

        public override MinigameView CreateView(Transform parent)
        {
            if (viewPrefab == null)
            {
                Debug.LogError($"[Shell Game]: View prefab is not linked on \"{name}\".", this);

                return null;
            }

            var view = Instantiate(viewPrefab, parent);
            view.Configure(BuildSettings());

            return view;
        }

        private ShellSettings BuildSettings()
        {
            return new ShellSettings
            {
                TableSprite = tableSprite,
                ShellSprite = shellSprite,
                SlotsRect = slotsRect,
                ShellScale = shellScale,
                PrizeScale = prizeScale,
                Difficulties = difficulties
            };
        }
    }
}
