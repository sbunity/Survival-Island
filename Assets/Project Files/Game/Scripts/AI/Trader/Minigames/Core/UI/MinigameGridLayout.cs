using UnityEngine;

namespace Watermelon
{
    public readonly struct MinigameGridLayout
    {
        public readonly Vector2 FieldSize;

        public readonly Vector2 GridSize;
        public readonly Vector2 GridCenter;
        public readonly Vector2 CellSize;

        public readonly float CellExtent;

        public readonly int Columns;
        public readonly int Rows;

        public bool IsValid => Columns > 0 && Rows > 0 && CellSize.x > 0f && CellSize.y > 0f;

        public MinigameGridLayout(Vector2 available, float aspect, Rect gridRect, int columns, int rows, float cellScale)
        {
            Columns = Mathf.Max(1, columns);
            Rows = Mathf.Max(1, rows);

            if (aspect <= 0f)
                aspect = 1f;

            var width = Mathf.Max(0f, available.x);
            var height = width / aspect;

            if (height > available.y)
            {
                height = Mathf.Max(0f, available.y);
                width = height * aspect;
            }

            FieldSize = new Vector2(width, height);

            GridSize = new Vector2(width * Mathf.Max(0.01f, gridRect.width), height * Mathf.Max(0.01f, gridRect.height));
            GridCenter = new Vector2(width * (gridRect.center.x - 0.5f), height * (gridRect.center.y - 0.5f));
            CellSize = new Vector2(GridSize.x / Columns, GridSize.y / Rows);
            CellExtent = Mathf.Min(CellSize.x, CellSize.y) * Mathf.Max(0.05f, cellScale);
        }

        public Vector2 CellToPosition(Vector2Int cell) => CellToPosition(cell.x, cell.y);

        public Vector2 CellToPosition(int x, int y)
        {
            return new Vector2(
                GridCenter.x - GridSize.x * 0.5f + CellSize.x * (x + 0.5f),
                GridCenter.y + GridSize.y * 0.5f - CellSize.y * (y + 0.5f));
        }

        public bool TryGetCell(Vector2 localPoint, out Vector2Int cell)
        {
            cell = Vector2Int.zero;

            if (!IsValid)
                return false;

            var local = localPoint - GridCenter;

            var x = Mathf.FloorToInt((local.x + GridSize.x * 0.5f) / CellSize.x);
            var y = Mathf.FloorToInt((GridSize.y * 0.5f - local.y) / CellSize.y);

            if (x < 0 || x >= Columns || y < 0 || y >= Rows)
                return false;

            cell = new Vector2Int(x, y);

            return true;
        }

        public static float GetAspect(Sprite sprite)
        {
            if (sprite == null || sprite.rect.height <= 0f)
                return 1f;

            return sprite.rect.width / sprite.rect.height;
        }
    }
}
