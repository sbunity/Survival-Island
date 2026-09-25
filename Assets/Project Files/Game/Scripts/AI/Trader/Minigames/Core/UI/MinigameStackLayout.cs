using UnityEngine;

namespace Watermelon
{
    [DisallowMultipleComponent]
    public class MinigameStackLayout : MonoBehaviour
    {
        [BoxGroup("Header", "Header")]
        [SerializeField] RectTransform header;
        [BoxGroup("Header")]
        [SerializeField] float headerHeight = 220f;
        [BoxGroup("Header")]
        [SerializeField] float headerTopGap;

        [BoxGroup("Body", "Body")]
        [SerializeField] RectTransform body;
        [BoxGroup("Body")]
        [SerializeField] float bodyTopGap;
        [BoxGroup("Body")]
        [SerializeField] float bodyBottomGap;
        [BoxGroup("Body")]
        [SerializeField] float bodySideGap;

        [BoxGroup("Footer", "Footer")]
        [SerializeField] RectTransform footer;
        [BoxGroup("Footer")]
        [SerializeField] float footerHeight;
        [BoxGroup("Footer")]
        [SerializeField] float footerBottomGap;

        [BoxGroup("Scaling", "Scaling")]
        [SerializeField, Min(1f)] float referenceHeight = 1440f;
        [BoxGroup("Scaling")]
        [SerializeField, Range(0f, 1f)] float minGapScale = 0.3f;
        [BoxGroup("Scaling")]
        [SerializeField, Min(0f)] float minBodyHeight = 320f;

        public float GapScale { get; private set; } = 1f;

        private RectTransform rectTransform;

        private void Awake() => Cache();

        private void OnEnable()
        {
            Cache();
            Apply();
        }

        private void OnRectTransformDimensionsChange() => Apply();

        private void Cache()
        {
            if (rectTransform == null)
                rectTransform = (RectTransform)transform;
        }

        public void Apply()
        {
            Cache();

            var available = rectTransform.rect.height;

            if (available <= 0f)
                return;

            GapScale = ResolveGapScale(available);

            var top = 0f;

            if (header != null)
            {
                top = headerTopGap * GapScale;

                PlaceTop(header, top, headerHeight);

                top += headerHeight;
            }

            var bottom = 0f;

            if (footer != null)
            {
                bottom = footerBottomGap * GapScale;

                PlaceBottom(footer, bottom, footerHeight);

                bottom += footerHeight;
            }

            if (body != null)
                Stretch(body, top + bodyTopGap * GapScale, bottom + bodyBottomGap * GapScale, bodySideGap);
        }

        private float ResolveGapScale(float available)
        {
            var scale = Mathf.Clamp(available / referenceHeight, minGapScale, 1f);

            var fixedHeight = (header != null ? headerHeight : 0f) + (footer != null ? footerHeight : 0f);
            var gapTotal = GetTotalGap();

            if (gapTotal <= 0f)
                return scale;

            var allowed = available - fixedHeight - minBodyHeight;

            if (allowed >= gapTotal * scale)
                return scale;

            return Mathf.Clamp01(allowed / gapTotal);
        }

        private float GetTotalGap()
        {
            var total = 0f;

            if (header != null)
                total += headerTopGap;

            if (footer != null)
                total += footerBottomGap;

            if (body != null)
                total += bodyTopGap + bodyBottomGap;

            return total;
        }

        public static Vector2 FitGrid(RectTransform area, MinigameGridLayout grid, Vector2 position)
        {
            if (area == null || !grid.IsValid)
                return position;

            var frame = area.GetComponentInParent<MinigameStackLayout>();

            return frame != null ? frame.Fit(area, grid, position) : position;
        }

        private Vector2 Fit(RectTransform area, MinigameGridLayout grid, Vector2 position)
        {
            var safeTop = GetEdgeIn(area, header, true, area.rect.yMax);
            var safeBottom = GetEdgeIn(area, footer, false, area.rect.yMin);

            if (safeTop - safeBottom < grid.GridSize.y)
                return new Vector2(position.x, (safeTop + safeBottom) * 0.5f - grid.GridCenter.y);

            var overTop = grid.GetGridTop(position) - safeTop;

            if (overTop > 0f)
                position.y -= overTop;

            var underBottom = safeBottom - grid.GetGridBottom(position);

            if (underBottom > 0f)
                position.y += underBottom;

            return position;
        }

        private float GetEdgeIn(RectTransform area, RectTransform slot, bool isHeader, float fallback)
        {
            if (slot == null || !slot.gameObject.activeInHierarchy)
                return fallback;

            var found = false;
            var edge = isHeader ? float.MaxValue : float.MinValue;

            for (var i = 0; i < slot.childCount; i++)
            {
                var child = (RectTransform)slot.GetChild(i);

                if (!child.gameObject.activeSelf)
                    continue;

                var value = GetEdge(area, child, isHeader);

                edge = isHeader ? Mathf.Min(edge, value) : Mathf.Max(edge, value);
                found = true;
            }

            return found ? edge : GetEdge(area, slot, isHeader);
        }

        private static readonly Vector3[] corners = new Vector3[4];

        private static float GetEdge(RectTransform area, RectTransform target, bool isHeader)
        {
            target.GetWorldCorners(corners);

            return area.InverseTransformPoint(corners[isHeader ? 0 : 1]).y;
        }

        private static void PlaceTop(RectTransform target, float offset, float height)
        {
            target.anchorMin = new Vector2(0f, 1f);
            target.anchorMax = new Vector2(1f, 1f);
            target.pivot = new Vector2(0.5f, 1f);
            target.offsetMin = new Vector2(0f, -offset - height);
            target.offsetMax = new Vector2(0f, -offset);
        }

        private static void PlaceBottom(RectTransform target, float offset, float height)
        {
            target.anchorMin = new Vector2(0f, 0f);
            target.anchorMax = new Vector2(1f, 0f);
            target.pivot = new Vector2(0.5f, 0f);
            target.offsetMin = new Vector2(0f, offset);
            target.offsetMax = new Vector2(0f, offset + height);
        }

        private static void Stretch(RectTransform target, float top, float bottom, float sideGap)
        {
            target.anchorMin = Vector2.zero;
            target.anchorMax = Vector2.one;
            target.pivot = new Vector2(0.5f, 0.5f);
            target.offsetMin = new Vector2(sideGap, bottom);
            target.offsetMax = new Vector2(-sideGap, -top);
        }
    }
}
