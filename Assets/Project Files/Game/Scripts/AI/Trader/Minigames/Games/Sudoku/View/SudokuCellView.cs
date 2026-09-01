using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public enum SudokuCellStyle
    {
        Empty = 0,
        Given = 1,
        Placed = 2,
        Wrong = 3
    }

    [RequireComponent(typeof(RectTransform))]
    public class SudokuCellView : MonoBehaviour
    {
        [SerializeField] Image backgroundImage;
        [SerializeField] Image highlightImage;
        [SerializeField] Image iconImage;

        private RectTransform rectTransform;

        private TweenCase scaleCase;
        private TweenCase shakeCase;
        private TweenCase highlightCase;

        private Vector2 origin;
        private bool hasOrigin;

        public int Symbol { get; private set; } = SudokuBoard.EMPTY;

        public SudokuCellStyle Style { get; private set; } = SudokuCellStyle.Empty;

        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                    rectTransform = (RectTransform)transform;

                return rectTransform;
            }
        }

        public void Resize(float size)
        {
            RectTransform.sizeDelta = new Vector2(size, size);
        }

        public void SetSymbol(int symbol, Sprite icon, SudokuCellStyle style, Color tint)
        {
            Symbol = symbol;
            Style = style;

            if (iconImage == null)
                return;

            iconImage.sprite = icon;
            iconImage.color = tint;
            iconImage.enabled = icon != null && style != SudokuCellStyle.Empty;
        }

        public void SetBackground(Sprite sprite, Color color)
        {
            if (backgroundImage == null)
                return;

            backgroundImage.sprite = sprite;
            backgroundImage.color = color;
            backgroundImage.enabled = sprite != null;
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

        public void PlayShake(float magnitude, float duration, SimpleCallback onComplete = null)
        {
            shakeCase.KillActive();

            origin = RectTransform.anchoredPosition;
            hasOrigin = true;

            shakeCase = RectTransform.DOAnchoredPositionShake(magnitude, duration).OnComplete(() =>
            {
                RestoreOrigin();

                onComplete?.Invoke();
            });
        }

        private void RestoreOrigin()
        {
            if (!hasOrigin)
                return;

            hasOrigin = false;

            RectTransform.anchoredPosition = origin;
        }

        public void KillTweens()
        {
            scaleCase.KillActive();
            shakeCase.KillActive();
            highlightCase.KillActive();

            RestoreOrigin();
        }

        private void OnDisable()
        {
            KillTweens();
        }
    }
}
