using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(RectTransform))]
    public class Match3TileView : MonoBehaviour
    {
        [SerializeField] Image iconImage;

        private RectTransform rectTransform;
        private TweenCase moveCase;
        private TweenCase scaleCase;

        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                    rectTransform = (RectTransform)transform;

                return rectTransform;
            }
        }

        public void Setup(Sprite icon, float size)
        {
            if (iconImage != null)
                iconImage.sprite = icon;

            RectTransform.sizeDelta = new Vector2(size, size);
        }

        public void Resize(float size)
        {
            RectTransform.sizeDelta = new Vector2(size, size);
        }

        public void PlaceAt(Vector2 position)
        {
            KillTweens();

            RectTransform.anchoredPosition = position;
            RectTransform.localScale = Vector3.one;
        }

        public void AnimateMove(Vector2 position, float duration, Ease.Type easing)
        {
            moveCase.KillActive();
            moveCase = RectTransform.DOAnchoredPosition(position, duration).SetEasing(easing);
        }

        public void AnimateSpawn(Vector2 position, float duration, Ease.Type easing)
        {
            KillTweens();

            RectTransform.anchoredPosition = position;
            RectTransform.localScale = Vector3.zero;

            scaleCase = RectTransform.DOScale(1f, duration).SetEasing(easing);
        }

        public void AnimateScale(float target, float duration, Ease.Type easing)
        {
            scaleCase.KillActive();
            scaleCase = RectTransform.DOScale(target, duration).SetEasing(easing);
        }

        public void AnimateClear(float duration, Ease.Type easing, SimpleCallback onComplete)
        {
            KillTweens();

            scaleCase = RectTransform.DOScale(0f, duration).SetEasing(easing).OnComplete(() => onComplete?.Invoke());
        }

        public void KillTweens()
        {
            moveCase.KillActive();
            scaleCase.KillActive();
        }

        private void OnDisable()
        {
            KillTweens();
        }
    }
}
