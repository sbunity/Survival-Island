using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(RectTransform))]
    public class TreasureCellView : MonoBehaviour
    {
        [SerializeField] Image sandImage;
        [SerializeField] Image highlightImage;
        [SerializeField] Image prizeImage;

        private RectTransform rectTransform;

        private TweenCase scaleCase;
        private TweenCase shakeCase;
        private TweenCase highlightCase;
        private TweenCase prizeCase;

        private Vector2 origin;
        private bool hasOrigin;

        public bool IsDug { get; private set; }

        public bool IsAnimating => scaleCase.ExistsAndActive() || shakeCase.ExistsAndActive() || prizeCase.ExistsAndActive();

        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                    rectTransform = (RectTransform)transform;

                return rectTransform;
            }
        }

        public void Resize(float size, float prizeScale, float highlightScale)
        {
            RectTransform.sizeDelta = new Vector2(size, size);

            if (prizeImage != null)
                prizeImage.rectTransform.sizeDelta = Vector2.one * (size * prizeScale);

            if (highlightImage != null)
                highlightImage.rectTransform.sizeDelta = Vector2.one * (size * highlightScale);
        }

        public void SetSand(Sprite sprite, bool isDug, Color tint)
        {
            IsDug = isDug;

            if (sandImage == null)
                return;

            sandImage.sprite = sprite;
            sandImage.color = tint;
            sandImage.enabled = sprite != null;
        }

        public void SetHighlight(Color color, float duration)
        {
            if (highlightImage == null)
                return;

            highlightCase.KillActive();

            if (duration <= 0f || color.a <= 0f)
            {
                highlightImage.color = color;
                highlightImage.enabled = color.a > 0f;

                return;
            }

            if (!highlightImage.enabled)
            {
                highlightImage.color = color.SetAlpha(0f);
                highlightImage.enabled = true;
            }

            highlightCase = highlightImage.DOColor(color, duration);
        }

        public void ShowPrize(Sprite sprite, float duration, Ease.Type easing)
        {
            if (prizeImage == null || sprite == null)
                return;

            prizeCase.KillActive();

            prizeImage.sprite = sprite;
            prizeImage.enabled = true;
            prizeImage.gameObject.SetActive(true);

            prizeImage.rectTransform.localScale = Vector3.zero;

            prizeCase = prizeImage.rectTransform.DOScale(1f, duration).SetEasing(easing);
        }

        public void HidePrize()
        {
            prizeCase.KillActive();

            if (prizeImage != null)
                prizeImage.gameObject.SetActive(false);
        }

        public void PlaceAt(Vector2 position)
        {
            KillTweens();

            hasOrigin = false;

            RectTransform.anchoredPosition = position;
            RectTransform.localScale = Vector3.one;
        }

        public void AnimateSpawn(Vector2 position, float duration, Ease.Type easing, float delay = 0f)
        {
            KillTweens();

            RectTransform.anchoredPosition = position;
            RectTransform.localScale = Vector3.zero;

            scaleCase = RectTransform.DOScale(1f, duration, delay).SetEasing(easing);
        }

        public void PlayPunch(float scale, float duration, Ease.Type easing)
        {
            scaleCase.KillActive();

            RectTransform.localScale = Vector3.one;

            scaleCase = RectTransform.DOPushScale(scale, 1f, duration, duration, easing, easing);
        }

        public void PlayShake(float magnitude, float duration)
        {
            shakeCase.KillActive();

            origin = RectTransform.anchoredPosition;
            hasOrigin = true;

            shakeCase = RectTransform.DOAnchoredPositionShake(magnitude, duration).OnComplete(RestoreOrigin);
        }

        public void KillTweens()
        {
            scaleCase.KillActive();
            shakeCase.KillActive();
            highlightCase.KillActive();
            prizeCase.KillActive();

            RestoreOrigin();
        }

        private void RestoreOrigin()
        {
            if (!hasOrigin)
                return;

            hasOrigin = false;

            RectTransform.anchoredPosition = origin;
        }

        private void OnDisable()
        {
            KillTweens();
        }
    }
}
