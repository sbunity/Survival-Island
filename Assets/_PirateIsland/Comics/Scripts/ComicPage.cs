using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ComicPage : MonoBehaviour
    {
        [SerializeField] List<ComicPanel> panels = new List<ComicPanel>();

        private CanvasGroup canvasGroup;
        private CanvasGroup CanvasGroup => canvasGroup != null ? canvasGroup : canvasGroup = GetComponent<CanvasGroup>();

        private RectTransform RectTransform => (RectTransform)transform;

        private TweenCase fadeTweenCase;
        private TweenCase moveTweenCase;

        public IReadOnlyList<ComicPanel> Panels => panels;

        public void Prepare()
        {
            KillTweens();

            foreach (ComicPanel panel in panels)
                panel.Prepare();

            gameObject.SetActive(false);
        }

        public void Show(float duration, float offset, SimpleCallback onCompleted)
        {
            KillTweens();
            gameObject.SetActive(true);

            CanvasGroup.alpha = 0f;
            RectTransform.anchoredPosition = new Vector2(offset, 0f);

            fadeTweenCase = CanvasGroup.DOFade(1f, duration, unscaledTime: true);
            moveTweenCase = RectTransform.DOAnchoredPosition(Vector2.zero, duration, unscaledTime: true).SetEasing(Ease.Type.CircOut).OnComplete(onCompleted);
        }

        public void Hide(float duration, float offset)
        {
            KillTweens();

            fadeTweenCase = CanvasGroup.DOFade(0f, duration, unscaledTime: true);
            moveTweenCase = RectTransform.DOAnchoredPosition(new Vector2(-offset, 0f), duration, unscaledTime: true).SetEasing(Ease.Type.CircIn).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }

        private void KillTweens()
        {
            fadeTweenCase.KillActive();
            moveTweenCase.KillActive();
        }

        private void OnDestroy()
        {
            KillTweens();
        }
    }
}
