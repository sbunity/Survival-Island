using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    public delegate void TreasureCellCallback(Vector2Int cell);

    public class TreasureFieldView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] RectTransform fieldRoot;
        [SerializeField] Image fieldImage;
        [SerializeField] RectTransform cellsRoot;
        [SerializeField] TreasureCellView cellPrefab;

        [BoxGroup("Animation", "Animation")]
        [SerializeField, Min(0.01f)] float spawnDuration = 0.22f;
        [BoxGroup("Animation")]
        [SerializeField, Min(0f)] float spawnStagger = 0.02f;
        [BoxGroup("Animation")]
        [SerializeField] Ease.Type spawnEasing = Ease.Type.BackOut;
        [BoxGroup("Animation")]
        [SerializeField, Min(0.01f)] float digDuration = 0.14f;
        [BoxGroup("Animation")]
        [SerializeField, Min(1f)] float digPunchScale = 1.2f;
        [BoxGroup("Animation")]
        [SerializeField] Ease.Type digEasing = Ease.Type.SineOut;
        [BoxGroup("Animation")]
        [SerializeField, Min(0.01f)] float rejectDuration = 0.2f;
        [BoxGroup("Animation")]
        [SerializeField, Min(1f)] float rejectShake = 8f;
        [BoxGroup("Animation")]
        [SerializeField, Min(0.01f)] float highlightDuration = 0.16f;

        [BoxGroup("Prize", "Prize")]
        [SerializeField, Min(0.01f)] float prizeAppearDuration = 0.24f;
        [BoxGroup("Prize")]
        [SerializeField] Ease.Type prizeAppearEasing = Ease.Type.BackOut;

        [BoxGroup("Colors", "Colors")]
        [SerializeField] Color noHighlightColor = new(1f, 1f, 1f, 0f);
        [BoxGroup("Colors")]
        [SerializeField, Range(0f, 1f)] float dugHighlightAlpha = 0.18f;
        [BoxGroup("Colors")]
        [SerializeField, Range(0.1f, 1.5f)] float highlightScale = 0.82f;
        [BoxGroup("Colors")]
        [SerializeField, Range(0f, 1f)] float dugTintStrength = 0.45f;

        public event TreasureCellCallback CellTapped;

        public bool IsInputEnabled { get; set; }

        public RectTransform CellsRoot => cellsRoot;

        public float SpawnDuration => spawnDuration + spawnStagger * Mathf.Max(0, CellCount - 1);

        public int CellCount => columns * rows;

        private TreasureHuntSettings settings;
        private MinigameGridLayout grid;

        private TreasureCellView[] cells;

        private int columns;
        private int rows;

        private Sprite prizeIcon;

        private Vector2 cellsOffset;
        private bool hasCellsOffset;

        public void Build(TreasureHuntSettings settings, int columns, int rows)
        {
            this.settings = settings;
            this.columns = Mathf.Max(TreasureBoard.MIN_SIZE, columns);
            this.rows = Mathf.Max(TreasureBoard.MIN_SIZE, rows);

            if (fieldImage != null)
                fieldImage.sprite = settings.FieldSprite;

            ClearCells();
            ApplyLayout();

            cells = new TreasureCellView[CellCount];

            for (var index = 0; index < cells.Length; index++)
            {
                var cell = ToCell(index);
                var view = Instantiate(cellPrefab, cellsRoot);

                view.gameObject.SetActive(false);
                view.Resize(grid.CellExtent, settings.PrizeScale, highlightScale);
                view.SetSand(settings.BuriedSprite, false, Color.white);
                view.SetHighlight(noHighlightColor, 0f);
                view.HidePrize();
                view.PlaceAt(grid.CellToPosition(cell));

                cells[index] = view;
            }
        }

        public void SetPrizeIcon(Sprite icon)
        {
            prizeIcon = icon;
        }

        public void SpawnCells()
        {
            if (cells == null)
                return;

            for (var index = 0; index < cells.Length; index++)
            {
                var view = cells[index];

                if (view == null)
                    continue;

                view.gameObject.SetActive(true);
                view.AnimateSpawn(grid.CellToPosition(ToCell(index)), spawnDuration, spawnEasing, spawnStagger * index);
            }
        }

        public void PlayDig(Vector2Int cell, Color highlightColor)
        {
            var view = GetCell(cell);

            if (view == null)
                return;

            view.SetSand(settings?.DugSprite, true, Color.Lerp(Color.white, highlightColor, dugTintStrength));
            view.SetHighlight(highlightColor.SetAlpha(dugHighlightAlpha), highlightDuration);
            view.PlayPunch(digPunchScale, digDuration, digEasing);
        }

        public void ShowPrize(Vector2Int cell)
        {
            var view = GetCell(cell);

            if (view != null)
                view.ShowPrize(prizeIcon, prizeAppearDuration, prizeAppearEasing);
        }

        public void PlayReject(Vector2Int cell)
        {
            var view = GetCell(cell);

            if (view != null)
                view.PlayShake(rejectShake, rejectDuration);
        }

        public void StopAllAnimations()
        {
            if (cells == null)
                return;

            for (var i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null)
                    cells[i].KillTweens();
            }
        }

        public void ApplyLayout()
        {
            if (fieldRoot == null || cellsRoot == null)
                return;

            CacheCellsOffset();

            var parent = fieldRoot.parent as RectTransform;
            var available = parent != null ? parent.rect.size : fieldRoot.rect.size;
            var aspect = MinigameGridLayout.GetAspect(fieldImage != null ? fieldImage.sprite : null);

            var gridRect = settings != null ? settings.GridRect : new Rect(0f, 0f, 1f, 1f);
            var cellScale = settings != null ? settings.CellScale : 1f;

            grid = new MinigameGridLayout(available, aspect, gridRect, columns, rows, cellScale);

            fieldRoot.sizeDelta = grid.FieldSize;

            cellsRoot.anchorMin = new Vector2(0.5f, 0.5f);
            cellsRoot.anchorMax = new Vector2(0.5f, 0.5f);
            cellsRoot.pivot = new Vector2(0.5f, 0.5f);
            cellsRoot.sizeDelta = grid.FieldSize;
            cellsRoot.anchoredPosition = cellsOffset;
        }

        public bool TryGetCell(Vector2 localPoint, out Vector2Int cell) => grid.TryGetCell(localPoint, out cell);

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsInputEnabled || cellsRoot == null)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(cellsRoot, eventData.position, eventData.pressEventCamera, out var localPoint))
                return;

            if (TryGetCell(localPoint, out var cell))
                CellTapped?.Invoke(cell);
        }

        private void CacheCellsOffset()
        {
            if (hasCellsOffset || cellsRoot == null)
                return;

            hasCellsOffset = true;
            cellsOffset = cellsRoot.anchoredPosition;
        }

        private Vector2Int ToCell(int index) => new(index % columns, index / columns);

        private TreasureCellView GetCell(Vector2Int cell)
        {
            if (cells == null || cell.x < 0 || cell.x >= columns || cell.y < 0 || cell.y >= rows)
                return null;

            return cells[cell.y * columns + cell.x];
        }

        private void ClearCells()
        {
            if (cells == null)
                return;

            for (var i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null)
                    Destroy(cells[i].gameObject);
            }

            cells = null;
        }

        private void OnRectTransformDimensionsChange()
        {
            if (settings == null || cells == null)
                return;

            for (var i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null && cells[i].IsAnimating)
                    return;
            }

            ApplyLayout();

            for (var index = 0; index < cells.Length; index++)
            {
                var view = cells[index];

                if (view == null)
                    continue;

                view.Resize(grid.CellExtent, settings.PrizeScale, highlightScale);
                view.PlaceAt(grid.CellToPosition(ToCell(index)));
            }
        }

        private void OnDisable()
        {
            StopAllAnimations();
        }
    }
}
