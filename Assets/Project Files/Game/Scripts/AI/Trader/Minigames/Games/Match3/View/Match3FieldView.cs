using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class Match3FieldView : MonoBehaviour
    {
        [SerializeField] RectTransform fieldRoot;
        [SerializeField] Image fieldImage;
        [SerializeField] RectTransform tilesRoot;
        [SerializeField] Match3TileView tilePrefab;

        [BoxGroup("Animation", "Animation")]
        [SerializeField, Min(0.01f)] float swapDuration = 0.18f;
        [BoxGroup("Animation")]
        [SerializeField, Min(0.01f)] float clearDuration = 0.14f;
        [BoxGroup("Animation")]
        [SerializeField, Min(0.01f)] float fallDurationPerCell = 0.07f;
        [BoxGroup("Animation")]
        [SerializeField, Min(0.01f)] float maxFallDuration = 0.32f;
        [BoxGroup("Animation")]
        [SerializeField, Min(0.01f)] float spawnDuration = 0.2f;
        [BoxGroup("Animation")]
        [SerializeField, Min(0f)] float spawnDelay = 0.1f;
        [BoxGroup("Animation")]
        [SerializeField, Min(0f)] float stepPause = 0.05f;

        [BoxGroup("Easing", "Easing")]
        [SerializeField] Ease.Type swapEasing = Ease.Type.SineInOut;
        [BoxGroup("Easing")]
        [SerializeField] Ease.Type fallEasing = Ease.Type.QuadIn;
        [BoxGroup("Easing")]
        [SerializeField] Ease.Type spawnEasing = Ease.Type.BackOut;
        [BoxGroup("Easing")]
        [SerializeField] Ease.Type clearEasing = Ease.Type.QuadIn;

        [BoxGroup("Selection", "Selection")]
        [SerializeField, Min(1f)] float selectedScale = 1.15f;
        [BoxGroup("Selection")]
        [SerializeField, Min(0.01f)] float selectionDuration = 0.12f;

        private Match3TileView[] tiles;
        private readonly Stack<Match3TileView> pool = new Stack<Match3TileView>();

        private Sprite[] tileIcons;
        private Match3Settings settings;

        private Vector2 gridSize;
        private Vector2 gridCenter;
        private Vector2 cellSize;
        private float tileSize;

        private bool hasSelection;
        private Vector2Int selectedCell;

        private TweenCase sequenceCase;

        public int Columns => settings != null ? settings.Columns : 0;
        public int Rows => settings != null ? settings.Rows : 0;
        public RectTransform TilesRoot => tilesRoot;

        public void Build(Match3Board board, Sprite[] icons, Match3Settings settings)
        {
            this.settings = settings;
            tileIcons = icons;

            if (fieldImage != null)
                fieldImage.sprite = settings.FieldSprite;

            ApplyLayout();

            ReleaseAll();

            tiles = new Match3TileView[settings.Columns * settings.Rows];

            for (var y = 0; y < settings.Rows; y++)
            {
                for (var x = 0; x < settings.Columns; x++)
                {
                    var cell = new Vector2Int(x, y);
                    var tile = Rent(board.Get(cell));

                    tile.PlaceAt(CellToPosition(cell));

                    SetTile(cell, tile);
                }
            }
        }

        public void ApplyLayout()
        {
            if (settings == null || fieldRoot == null)
                return;

            var parent = fieldRoot.parent as RectTransform;
            var available = parent != null ? parent.rect.size : fieldRoot.rect.size;

            var sprite = fieldImage != null ? fieldImage.sprite : null;
            var aspect = sprite != null && sprite.rect.height > 0f ? sprite.rect.width / sprite.rect.height : 1f;

            var width = available.x;
            var height = width / aspect;

            if (height > available.y)
            {
                height = available.y;
                width = height * aspect;
            }

            fieldRoot.sizeDelta = new Vector2(width, height);

            tilesRoot.anchorMin = new Vector2(0.5f, 0.5f);
            tilesRoot.anchorMax = new Vector2(0.5f, 0.5f);
            tilesRoot.pivot = new Vector2(0.5f, 0.5f);
            tilesRoot.anchoredPosition = Vector2.zero;
            tilesRoot.sizeDelta = new Vector2(width, height);

            var grid = settings.GridRect;

            gridSize = new Vector2(width * Mathf.Max(0.01f, grid.width), height * Mathf.Max(0.01f, grid.height));
            gridCenter = new Vector2(width * (grid.center.x - 0.5f), height * (grid.center.y - 0.5f));
            cellSize = new Vector2(gridSize.x / settings.Columns, gridSize.y / settings.Rows);
            tileSize = Mathf.Min(cellSize.x, cellSize.y) * Mathf.Max(0.05f, settings.TileScale);
        }

        public Vector2 CellToPosition(Vector2Int cell)
        {
            return new Vector2(
                gridCenter.x - gridSize.x * 0.5f + cellSize.x * (cell.x + 0.5f),
                gridCenter.y + gridSize.y * 0.5f - cellSize.y * (cell.y + 0.5f));
        }

        public bool TryGetCell(Vector2 localPoint, out Vector2Int cell)
        {
            cell = Vector2Int.zero;

            if (settings == null || cellSize.x <= 0f || cellSize.y <= 0f)
                return false;

            var local = localPoint - gridCenter;

            var x = Mathf.FloorToInt((local.x + gridSize.x * 0.5f) / cellSize.x);
            var y = Mathf.FloorToInt((gridSize.y * 0.5f - local.y) / cellSize.y);

            if (x < 0 || x >= settings.Columns || y < 0 || y >= settings.Rows)
                return false;

            cell = new Vector2Int(x, y);

            return true;
        }

        public void SetSelected(Vector2Int cell)
        {
            ClearSelection();

            var tile = GetTile(cell);
            if (tile == null)
                return;

            hasSelection = true;
            selectedCell = cell;

            tile.AnimateScale(selectedScale, selectionDuration, Ease.Type.SineOut);
        }

        public void ClearSelection()
        {
            if (!hasSelection)
                return;

            var tile = GetTile(selectedCell);
            if (tile != null)
                tile.AnimateScale(1f, selectionDuration, Ease.Type.SineOut);

            hasSelection = false;
        }

        public void PlayInvalidSwap(Vector2Int from, Vector2Int to, SimpleCallback onComplete)
        {
            ClearSelection();

            var first = GetTile(from);
            var second = GetTile(to);

            if (first == null || second == null)
            {
                onComplete?.Invoke();

                return;
            }

            first.AnimateMove(CellToPosition(to), swapDuration, swapEasing);
            second.AnimateMove(CellToPosition(from), swapDuration, swapEasing);

            Schedule(swapDuration, () =>
            {
                first.AnimateMove(CellToPosition(from), swapDuration, swapEasing);
                second.AnimateMove(CellToPosition(to), swapDuration, swapEasing);

                Schedule(swapDuration, onComplete);
            });
        }

        public void PlayResolution(Match3Resolution resolution, Match3StepCallback onStepResolved, SimpleCallback onComplete)
        {
            ClearSelection();

            SwapTiles(resolution.From, resolution.To);

            var first = GetTile(resolution.To);
            var second = GetTile(resolution.From);

            if (first != null)
                first.AnimateMove(CellToPosition(resolution.To), swapDuration, swapEasing);

            if (second != null)
                second.AnimateMove(CellToPosition(resolution.From), swapDuration, swapEasing);

            Schedule(swapDuration, () => PlayStep(resolution, 0, onStepResolved, onComplete));
        }

        public void PlayShuffle(Match3Board board, SimpleCallback onComplete)
        {
            ClearSelection();

            for (var i = 0; i < tiles.Length; i++)
                tiles[i]?.AnimateClear(clearDuration, clearEasing, null);

            Schedule(clearDuration, () =>
            {
                ReleaseAll();

                tiles = new Match3TileView[settings.Columns * settings.Rows];

                for (var y = 0; y < settings.Rows; y++)
                {
                    for (var x = 0; x < settings.Columns; x++)
                    {
                        var cell = new Vector2Int(x, y);
                        var tile = Rent(board.Get(cell));

                        tile.AnimateSpawn(CellToPosition(cell), spawnDuration, spawnEasing);

                        SetTile(cell, tile);
                    }
                }

                Schedule(spawnDuration, onComplete);
            });
        }

        public void StopAllAnimations()
        {
            sequenceCase.KillActive();

            if (tiles == null)
                return;

            for (var i = 0; i < tiles.Length; i++)
                tiles[i]?.KillTweens();
        }

        private void PlayStep(Match3Resolution resolution, int index, Match3StepCallback onStepResolved, SimpleCallback onComplete)
        {
            if (index >= resolution.Steps.Count)
            {
                onComplete?.Invoke();

                return;
            }

            var step = resolution.Steps[index];

            for (var i = 0; i < step.Cleared.Count; i++)
            {
                var cell = step.Cleared[i].Cell;
                var tile = GetTile(cell);

                SetTile(cell, null);

                if (tile != null)
                    tile.AnimateClear(clearDuration, clearEasing, () => Release(tile));
            }

            onStepResolved?.Invoke(step);

            Schedule(clearDuration, () =>
            {
                var fallDuration = ApplyMoves(step);

                var spawnStart = fallDuration + spawnDelay;

                ApplySpawns(step, spawnStart);

                var settleDuration = Mathf.Max(fallDuration, step.Spawns.Count > 0 ? spawnStart + spawnDuration : 0f) + stepPause;

                Schedule(settleDuration, () => PlayStep(resolution, index + 1, onStepResolved, onComplete));
            });
        }

        private float ApplyMoves(Match3Step step)
        {
            var longest = 0f;

            if (step.Moves.Count == 0)
                return longest;

            var moved = new Match3TileView[step.Moves.Count];

            for (var i = 0; i < step.Moves.Count; i++)
            {
                moved[i] = GetTile(step.Moves[i].From);
                SetTile(step.Moves[i].From, null);
            }

            for (var i = 0; i < step.Moves.Count; i++)
            {
                var move = step.Moves[i];
                var tile = moved[i];

                SetTile(move.To, tile);

                if (tile == null)
                    continue;

                var distance = Mathf.Abs(move.To.y - move.From.y);
                var duration = Mathf.Min(fallDurationPerCell * distance, maxFallDuration);

                tile.AnimateMove(CellToPosition(move.To), duration, fallEasing);

                longest = Mathf.Max(longest, duration);
            }

            return longest;
        }

        private void ApplySpawns(Match3Step step, float delay)
        {
            for (var i = 0; i < step.Spawns.Count; i++)
            {
                var spawn = step.Spawns[i];
                var tile = Rent(spawn.TileId);

                tile.AnimateSpawn(CellToPosition(spawn.Cell), spawnDuration, spawnEasing, delay);

                SetTile(spawn.Cell, tile);
            }
        }

        private void SwapTiles(Vector2Int a, Vector2Int b)
        {
            var first = GetTile(a);
            var second = GetTile(b);

            SetTile(a, second);
            SetTile(b, first);
        }

        private Match3TileView GetTile(Vector2Int cell)
        {
            var index = ToIndex(cell);

            return index >= 0 ? tiles[index] : null;
        }

        private void SetTile(Vector2Int cell, Match3TileView tile)
        {
            var index = ToIndex(cell);

            if (index >= 0)
                tiles[index] = tile;
        }

        private int ToIndex(Vector2Int cell)
        {
            if (tiles == null || settings == null)
                return -1;

            if (cell.x < 0 || cell.x >= settings.Columns || cell.y < 0 || cell.y >= settings.Rows)
                return -1;

            return cell.y * settings.Columns + cell.x;
        }

        private Match3TileView Rent(int tileId)
        {
            var tile = pool.Count > 0 ? pool.Pop() : Instantiate(tilePrefab, tilesRoot);

            tile.gameObject.SetActive(true);
            tile.transform.SetParent(tilesRoot, false);
            tile.Setup(GetIcon(tileId), tileSize);

            return tile;
        }

        private void Release(Match3TileView tile)
        {
            if (tile == null)
                return;

            tile.KillTweens();
            tile.gameObject.SetActive(false);

            pool.Push(tile);
        }

        private void ReleaseAll()
        {
            if (tiles == null)
                return;

            for (var i = 0; i < tiles.Length; i++)
            {
                Release(tiles[i]);
                tiles[i] = null;
            }
        }

        private Sprite GetIcon(int tileId)
        {
            if (tileIcons == null || tileId < 0 || tileId >= tileIcons.Length)
                return null;

            return tileIcons[tileId];
        }

        private void Schedule(float delay, SimpleCallback callback)
        {
            sequenceCase.KillActive();
            sequenceCase = Tween.DelayedCall(delay, callback);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (settings == null || tiles == null || sequenceCase.ExistsAndActive())
                return;

            ApplyLayout();

            for (var y = 0; y < settings.Rows; y++)
            {
                for (var x = 0; x < settings.Columns; x++)
                {
                    var cell = new Vector2Int(x, y);
                    var tile = GetTile(cell);

                    if (tile == null)
                        continue;

                    tile.Resize(tileSize);
                    tile.PlaceAt(CellToPosition(cell));
                }
            }
        }

        private void OnDisable()
        {
            StopAllAnimations();
        }
    }
}
