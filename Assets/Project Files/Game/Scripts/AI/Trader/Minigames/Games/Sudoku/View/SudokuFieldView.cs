using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class SudokuFieldView : MonoBehaviour
    {
        [SerializeField] RectTransform fieldRoot;
        [SerializeField] Image fieldImage;
        [SerializeField] RectTransform cellsRoot;
        [SerializeField] SudokuCellView cellPrefab;

        [BoxGroup("Animation", "Animation")]
        [SerializeField, Min(0.01f)] float spawnDuration = 0.22f;
        [BoxGroup("Animation")]
        [SerializeField, Min(0f)] float spawnStagger = 0.02f;
        [BoxGroup("Animation")]
        [SerializeField] Ease.Type spawnEasing = Ease.Type.BackOut;
        [BoxGroup("Animation")]
        [SerializeField, Min(0.01f)] float placeDuration = 0.11f;
        [BoxGroup("Animation")]
        [SerializeField, Min(1f)] float placePunchScale = 1.25f;
        [BoxGroup("Animation")]
        [SerializeField] Ease.Type placeEasing = Ease.Type.SineOut;
        [BoxGroup("Animation")]
        [SerializeField, Min(0.01f)] float mistakeDuration = 0.4f;
        [BoxGroup("Animation")]
        [SerializeField, Min(1f)] float mistakeShake = 12f;
        [BoxGroup("Animation")]
        [SerializeField, Min(0.01f)] float highlightDuration = 0.12f;

        [BoxGroup("Cells", "Cells")]
        [SerializeField] Sprite cellBackgroundSprite;
        [BoxGroup("Cells")]
        [SerializeField] Color cellBackgroundColor = Color.white;
        [BoxGroup("Cells")]
        [SerializeField] Color givenColor = Color.white;
        [BoxGroup("Cells")]
        [SerializeField] Color placedColor = Color.white;
        [BoxGroup("Cells")]
        [SerializeField] Color wrongColor = new Color(1f, 0.45f, 0.4f, 1f);

        [BoxGroup("Highlights", "Highlights")]
        [SerializeField] Color noHighlightColor = new Color(1f, 1f, 1f, 0f);
        [BoxGroup("Highlights")]
        [SerializeField] Color selectionColor = new Color(1f, 0.92f, 0.45f, 0.55f);
        [BoxGroup("Highlights")]
        [SerializeField] Color matchColor = new Color(1f, 1f, 1f, 0.28f);
        [BoxGroup("Highlights")]
        [SerializeField] Color errorColor = new Color(1f, 0.3f, 0.28f, 0.6f);

        private SudokuCellView[] cells;
        private readonly Stack<SudokuCellView> pool = new Stack<SudokuCellView>();

        private Sprite[] icons;
        private SudokuSettings settings;
        private SudokuLayout layout;
        private MinigameGridLayout grid;

        private int highlightedSymbol = SudokuBoard.EMPTY;

        private bool hasSelection;
        private Vector2Int selectedCell;

        private TweenCase sequenceCase;

        public RectTransform CellsRoot => cellsRoot;

        public float MistakeDuration => mistakeDuration;

        public void Build(Sprite[] icons, SudokuSettings settings, SudokuLayout layout)
        {
            this.icons = icons;
            this.settings = settings;
            this.layout = layout;

            if (fieldImage != null)
                fieldImage.sprite = settings.FieldSprite;

            ApplyLayout();

            ReleaseAll();

            cells = new SudokuCellView[layout.CellCount];

            highlightedSymbol = SudokuBoard.EMPTY;
            hasSelection = false;
        }

        public void SpawnCells(SudokuBoard board, bool animated = true)
        {
            if (cells == null || board == null)
                return;

            ReleaseAll();

            var spawnIndex = 0;

            for (var y = 0; y < layout.Size; y++)
            {
                for (var x = 0; x < layout.Size; x++)
                {
                    var cell = new Vector2Int(x, y);
                    var view = Rent();

                    var symbol = board.Get(cell);
                    var style = symbol == SudokuBoard.EMPTY
                        ? SudokuCellStyle.Empty
                        : board.IsGiven(cell) ? SudokuCellStyle.Given : SudokuCellStyle.Placed;

                    view.SetSymbol(symbol, GetIcon(symbol), style, GetTint(style));
                    view.SetHighlight(noHighlightColor, 0f);

                    if (animated && style != SudokuCellStyle.Empty)
                        view.AnimateSpawn(grid.CellToPosition(cell), spawnDuration, spawnEasing, spawnStagger * spawnIndex++);
                    else
                        view.PlaceAt(grid.CellToPosition(cell));

                    SetCell(cell, view);
                }
            }
        }

        public void ApplyLayout()
        {
            if (fieldRoot == null || cellsRoot == null)
                return;

            var parent = fieldRoot.parent as RectTransform;
            var available = parent != null ? parent.rect.size : fieldRoot.rect.size;
            var aspect = MinigameGridLayout.GetAspect(fieldImage != null ? fieldImage.sprite : null);

            var gridRect = settings != null ? settings.GridRect : new Rect(0f, 0f, 1f, 1f);
            var cellScale = settings != null ? settings.CellScale : 1f;

            grid = new MinigameGridLayout(available, aspect, gridRect, layout.Size, layout.Size, cellScale);

            fieldRoot.sizeDelta = grid.FieldSize;

            cellsRoot.anchorMin = new Vector2(0.5f, 0.5f);
            cellsRoot.anchorMax = new Vector2(0.5f, 0.5f);
            cellsRoot.pivot = new Vector2(0.5f, 0.5f);
            cellsRoot.anchoredPosition = Vector2.zero;
            cellsRoot.sizeDelta = grid.FieldSize;
        }

        public bool TryGetCell(Vector2 localPoint, out Vector2Int cell) => grid.TryGetCell(localPoint, out cell);

        public void SetSelectedCell(Vector2Int cell)
        {
            hasSelection = true;
            selectedCell = cell;

            RefreshHighlights();
        }

        public void ClearSelectedCell()
        {
            if (!hasSelection)
                return;

            hasSelection = false;

            RefreshHighlights();
        }

        public void SetHighlightedSymbol(int symbol)
        {
            highlightedSymbol = symbol;

            RefreshHighlights();
        }

        public void PlayPlace(Vector2Int cell, int symbol)
        {
            var view = GetCell(cell);
            if (view == null)
                return;

            view.SetSymbol(symbol, GetIcon(symbol), SudokuCellStyle.Placed, GetTint(SudokuCellStyle.Placed));
            view.PlayPunch(placePunchScale, placeDuration, placeEasing);

            RefreshHighlights();
        }

        public void PlayMistake(Vector2Int cell, int symbol, bool hasConflict, Vector2Int conflict, SimpleCallback onComplete)
        {
            var view = GetCell(cell);

            if (view == null)
            {
                onComplete?.Invoke();

                return;
            }

            view.SetSymbol(symbol, GetIcon(symbol), SudokuCellStyle.Wrong, GetTint(SudokuCellStyle.Wrong));
            view.SetHighlight(errorColor, highlightDuration);
            view.PlayShake(mistakeShake, mistakeDuration);

            var conflictView = hasConflict ? GetCell(conflict) : null;

            if (conflictView != null)
                conflictView.SetHighlight(errorColor, highlightDuration);

            Schedule(mistakeDuration, () =>
            {
                view.SetSymbol(SudokuBoard.EMPTY, null, SudokuCellStyle.Empty, GetTint(SudokuCellStyle.Empty));

                RefreshHighlights();

                onComplete?.Invoke();
            });
        }

        public void PlayReject(Vector2Int cell)
        {
            var view = GetCell(cell);

            if (view != null)
                view.PlayShake(mistakeShake * 0.5f, mistakeDuration * 0.5f);
        }

        public void StopAllAnimations()
        {
            sequenceCase.KillActive();

            if (cells == null)
                return;

            for (var i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null)
                    cells[i].KillTweens();
            }
        }

        private void RefreshHighlights()
        {
            if (cells == null)
                return;

            for (var i = 0; i < cells.Length; i++)
            {
                var view = cells[i];

                if (view == null || view.Style == SudokuCellStyle.Wrong)
                    continue;

                var color = noHighlightColor;

                if (highlightedSymbol != SudokuBoard.EMPTY && view.Symbol == highlightedSymbol)
                    color = matchColor;

                if (hasSelection && layout.ToCell(i) == selectedCell)
                    color = selectionColor;

                view.SetHighlight(color, highlightDuration);
            }
        }

        private Color GetTint(SudokuCellStyle style)
        {
            return style switch
            {
                SudokuCellStyle.Given => givenColor,
                SudokuCellStyle.Wrong => wrongColor,
                _ => placedColor,
            };
        }

        private Sprite GetIcon(int symbol)
        {
            if (icons == null || symbol < 0 || symbol >= icons.Length)
                return null;

            return icons[symbol];
        }

        private SudokuCellView GetCell(Vector2Int cell)
        {
            var index = ToIndex(cell);

            return index >= 0 ? cells[index] : null;
        }

        private void SetCell(Vector2Int cell, SudokuCellView view)
        {
            var index = ToIndex(cell);

            if (index >= 0)
                cells[index] = view;
        }

        private int ToIndex(Vector2Int cell)
        {
            if (cells == null || !layout.IsInside(cell))
                return -1;

            return layout.ToIndex(cell);
        }

        private SudokuCellView Rent()
        {
            var view = pool.Count > 0 ? pool.Pop() : Instantiate(cellPrefab, cellsRoot);

            view.gameObject.SetActive(true);
            view.transform.SetParent(cellsRoot, false);
            view.Resize(grid.CellExtent);
            view.SetBackground(cellBackgroundSprite, cellBackgroundColor);

            return view;
        }

        private void Release(SudokuCellView view)
        {
            if (view == null)
                return;

            view.KillTweens();
            view.gameObject.SetActive(false);

            pool.Push(view);
        }

        private void ReleaseAll()
        {
            if (cells == null)
                return;

            for (var i = 0; i < cells.Length; i++)
            {
                Release(cells[i]);

                cells[i] = null;
            }
        }

        private void Schedule(float delay, SimpleCallback callback)
        {
            sequenceCase.KillActive();
            sequenceCase = Tween.DelayedCall(delay, callback);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (settings == null || cells == null || sequenceCase.ExistsAndActive())
                return;

            ApplyLayout();

            for (var i = 0; i < cells.Length; i++)
            {
                var view = cells[i];

                if (view == null)
                    continue;

                view.Resize(grid.CellExtent);
                view.PlaceAt(grid.CellToPosition(layout.ToCell(i)));
            }
        }

        private void OnDisable()
        {
            StopAllAnimations();
        }
    }
}
