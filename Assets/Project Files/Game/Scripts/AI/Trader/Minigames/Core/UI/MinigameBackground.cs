using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(Image))]
    public class MinigameBackground : MonoBehaviour
    {
        [SerializeField] Image image;
        [SerializeField] bool coverParent = true;

        private RectTransform rectTransform;
        private RectTransform parentRect;

        public void SetSprite(Sprite sprite)
        {
            Cache();

            image.sprite = sprite;
            image.enabled = sprite != null;

            ApplyCover();
        }

        private void OnEnable()
        {
            Cache();
            ApplyCover();
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyCover();
        }

        private void Cache()
        {
            if (image == null)
                image = GetComponent<Image>();

            if (rectTransform == null)
                rectTransform = (RectTransform)transform;

            if (parentRect == null)
                parentRect = transform.parent as RectTransform;
        }

        private void ApplyCover()
        {
            if (!coverParent || image == null || image.sprite == null || parentRect == null)
                return;

            var available = parentRect.rect.size;
            if (available.x <= 0f || available.y <= 0f)
                return;

            var spriteSize = image.sprite.rect.size;
            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
                return;

            var scale = Mathf.Max(available.x / spriteSize.x, available.y / spriteSize.y);
            var target = spriteSize * scale;

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = target;
        }
    }
}
