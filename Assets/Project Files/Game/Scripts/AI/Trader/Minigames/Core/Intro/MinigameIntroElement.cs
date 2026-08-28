using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MinigameIntroElement : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;

        [SerializeField, Min(0.01f)] float duration = 0.25f;
        [SerializeField, Range(0.1f, 1f)] float hiddenScale = 0.85f;
        [SerializeField] Ease.Type easing = Ease.Type.BackOut;

        private RectTransform rectTransform;

        private TweenCase fadeCase;
        private TweenCase scaleCase;

        public bool IsRevealed { get; private set; } = true;

        public void Hide()
        {
            Cache();
            KillTweens();

            IsRevealed = false;

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            rectTransform.localScale = Vector3.one * hiddenScale;
        }

        public void Reveal()
        {
            Cache();
            KillTweens();

            IsRevealed = true;

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            fadeCase = canvasGroup.DOFade(1f, duration);
            scaleCase = rectTransform.DOScale(1f, duration).SetEasing(easing);
        }

        public void ShowInstantly()
        {
            Cache();
            KillTweens();

            IsRevealed = true;

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            rectTransform.localScale = Vector3.one;
        }

        private void Cache()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (rectTransform == null)
                rectTransform = (RectTransform)transform;
        }

        private void KillTweens()
        {
            fadeCase.KillActive();
            scaleCase.KillActive();
        }

        private void OnDisable()
        {
            KillTweens();
        }
    }
}
