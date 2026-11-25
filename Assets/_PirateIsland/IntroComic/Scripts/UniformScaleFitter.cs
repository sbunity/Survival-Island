using UnityEngine;

namespace Watermelon
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class UniformScaleFitter : MonoBehaviour
    {
        [SerializeField] RectTransform content;

        private void OnEnable()
        {
            Fit();
        }

        private void OnRectTransformDimensionsChange()
        {
            Fit();
        }

        private void Fit()
        {
            if (content == null) return;

            Rect area = ((RectTransform)transform).rect;
            Vector2 contentSize = content.rect.size;
            if (contentSize.x <= 0f || contentSize.y <= 0f) return;

            float scale = Mathf.Min(area.width / contentSize.x, area.height / contentSize.y);
            if (scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale)) return;

            content.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
