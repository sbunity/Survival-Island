using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class Match3Hud : MonoBehaviour
    {
        [SerializeField] Image goalIcon;
        [SerializeField] TMP_Text goalText;
        [SerializeField] TMP_Text movesText;

        [BoxGroup("Captions", "Captions")]
        [SerializeField] string goalFormat = "Collect {0}/{1}";
        [BoxGroup("Captions")]
        [SerializeField] string movesFormat = "Moves left: {0}";

        [BoxGroup("Feedback", "Feedback")]
        [SerializeField, Min(1f)] float punchScale = 1.2f;
        [BoxGroup("Feedback")]
        [SerializeField, Min(0.01f)] float punchDuration = 0.12f;

        private TweenCase punchCase;

        public void Setup(Sprite icon, int collected, int required, int moves)
        {
            if (goalIcon != null)
            {
                goalIcon.sprite = icon;
                goalIcon.enabled = icon != null;
            }

            SetProgress(collected, required, false);
            SetMoves(moves);
        }

        public void SetProgress(int collected, int required, bool punch = true)
        {
            if (goalText != null)
                goalText.text = string.Format(goalFormat, Mathf.Min(collected, required), required);

            if (punch && goalIcon != null)
                Punch(goalIcon.transform);
        }

        public void SetMoves(int moves)
        {
            if (movesText != null)
                movesText.text = string.Format(movesFormat, Mathf.Max(0, moves));
        }

        private void Punch(Transform target)
        {
            punchCase.KillActive();

            target.localScale = Vector3.one;

            punchCase = target.DOScale(punchScale, punchDuration).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                punchCase = target.DOScale(1f, punchDuration).SetEasing(Ease.Type.SineIn);
            });
        }

        private void OnDisable()
        {
            punchCase.KillActive();
        }
    }
}
