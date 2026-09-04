using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ShellDifficulty : MinigameDifficulty
    {
        [SerializeField, Min(ShellBoard.MIN_SLOTS)] int shellCount = 3;
        public int ShellCount => Mathf.Max(ShellBoard.MIN_SLOTS, shellCount);

        [SerializeField] DuoInt swapCount = new(7, 10);

        [SerializeField, Min(0.05f)] float swapDuration = 0.42f;
        public float SwapDuration => swapDuration;

        [SerializeField, Min(0f)] float swapInterval = 0.07f;
        public float SwapInterval => swapInterval;

        public int RollSwapCount(System.Random random) => Mathf.Max(1, random.Range(swapCount));
    }
}
