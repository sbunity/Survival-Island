using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ComicPanel : MonoBehaviour
    {
        [SerializeField] RectTransform artRectTransform;
        [SerializeField] List<ComicBubble> bubbles = new List<ComicBubble>();

        [Space]
        [SerializeField] float revealDuration = 0.35f;
        [SerializeField] float hiddenScale = 0.94f;

        [SerializeField] float artStartScale = 1.06f;
        [SerializeField] float artSettleDuration = 6f;

        private CanvasGroup canvasGroup;
        private CanvasGroup CanvasGroup => canvasGroup != null ? canvasGroup : canvasGroup = GetComponent<CanvasGroup>();

        private TweenCase fadeTweenCase;
        private TweenCase scaleTweenCase;
        private TweenCase artTweenCase;

        private SimpleCallback completedCallback;

        public IReadOnlyList<ComicBubble> Bubbles => bubbles;

        public void Prepare()
        {
            KillTweens();
            completedCallback = null;

            CanvasGroup.alpha = 0f;
            transform.localScale = Vector3.one * hiddenScale;

            if (artRectTransform != null)
                artRectTransform.localScale = Vector3.one * artStartScale;

            foreach (ComicBubble bubble in bubbles)
                bubble.Prepare();
        }

        public void Reveal(SimpleCallback onCompleted)
        {
            Prepare();
            completedCallback = onCompleted;

            fadeTweenCase = CanvasGroup.DOFade(1f, revealDuration);
            scaleTweenCase = transform.DOScale(Vector3.one, revealDuration).SetEasing(Ease.Type.BackOut).OnComplete(NotifyCompleted);

            if (artRectTransform != null)
                artTweenCase = artRectTransform.DOScale(Vector3.one, artSettleDuration).SetEasing(Ease.Type.SineOut);
        }

        public void Complete()
        {
            fadeTweenCase.KillActive();
            scaleTweenCase.KillActive();

            CanvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;

            NotifyCompleted();
        }

        private void NotifyCompleted()
        {
            SimpleCallback callback = completedCallback;
            completedCallback = null;
            callback?.Invoke();
        }

        private void KillTweens()
        {
            fadeTweenCase.KillActive();
            scaleTweenCase.KillActive();
            artTweenCase.KillActive();
        }

        private void OnDestroy()
        {
            KillTweens();
        }
    }
}
