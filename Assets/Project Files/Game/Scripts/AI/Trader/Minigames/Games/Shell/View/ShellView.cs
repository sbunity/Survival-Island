using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    public delegate void ShellTappedCallback(ShellView shell);

    [RequireComponent(typeof(RectTransform))]
    public class ShellView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Image image;

        public event ShellTappedCallback Tapped;

        public int Slot { get; set; }

        public bool IsInteractable { get; set; }

        private RectTransform rectTransform;

        private Vector2 restPosition;

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

        public bool IsMoving => moveCase.ExistsAndActive();

        public void SetSprite(Sprite sprite)
        {
            if (image == null)
                return;

            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        public void Resize(Vector2 size)
        {
            RectTransform.sizeDelta = size;
        }

        public void PlaceAt(Vector2 position)
        {
            KillTweens();

            restPosition = position;

            RectTransform.anchoredPosition = position;
            RectTransform.localScale = Vector3.one;
        }

        public void AnimateSpawn(Vector2 position, float duration, Ease.Type easing, float delay = 0f)
        {
            KillTweens();

            restPosition = position;

            RectTransform.anchoredPosition = position;
            RectTransform.localScale = Vector3.zero;

            scaleCase = RectTransform.DOScale(1f, duration, delay).SetEasing(easing);
        }

        public TweenCase MoveTo(Vector2 position, float duration, float arcHeight, Ease.Type easing)
        {
            restPosition = position;

            return Travel(position, duration, arcHeight, easing);
        }

        public TweenCase Lift(float height, float duration, Ease.Type easing)
        {
            return Travel(restPosition + new Vector2(0f, height), duration, 0f, easing);
        }

        public TweenCase Drop(float duration, Ease.Type easing)
        {
            return Travel(restPosition, duration, 0f, easing);
        }

        public void PlayPunch(float scale, float duration, Ease.Type easing)
        {
            scaleCase.KillActive();

            RectTransform.localScale = Vector3.one;

            scaleCase = RectTransform.DOPushScale(scale, 1f, duration, duration, easing, easing);
        }

        public void KillTweens()
        {
            moveCase.KillActive();
            scaleCase.KillActive();
        }

        private TweenCase Travel(Vector2 target, float duration, float arcHeight, Ease.Type easing)
        {
            moveCase.KillActive();

            var start = RectTransform.anchoredPosition;

            moveCase = Tween.DoFloat(0f, 1f, duration, progress =>
            {
                var position = Vector2.LerpUnclamped(start, target, progress);

                if (arcHeight != 0f)
                    position.y += arcHeight * Mathf.Sin(Mathf.Clamp01(progress) * Mathf.PI);

                RectTransform.anchoredPosition = position;
            }).SetEasing(easing);

            return moveCase;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsInteractable)
                return;

            Tapped?.Invoke(this);
        }

        private void OnDisable()
        {
            KillTweens();
        }
    }
}
