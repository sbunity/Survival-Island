using UnityEngine;

namespace Watermelon
{
    public readonly struct SudokuLayout
    {
        public const int MIN_SIZE = 2;

        public readonly int Size;

        public readonly int BoxWidth;
        public readonly int BoxHeight;

        public int SymbolCount => Size;
        public int CellCount => Size * Size;

        public bool HasBoxes => BoxWidth > 1 && BoxHeight > 1 && BoxWidth * BoxHeight == Size && Size % BoxWidth == 0 && Size % BoxHeight == 0;

        public SudokuLayout(int size, int boxWidth = 0, int boxHeight = 0)
        {
            Size = Mathf.Max(MIN_SIZE, size);
            BoxWidth = Mathf.Max(0, boxWidth);
            BoxHeight = Mathf.Max(0, boxHeight);
        }

        public bool IsInside(int x, int y) => x >= 0 && x < Size && y >= 0 && y < Size;
        public bool IsInside(Vector2Int cell) => IsInside(cell.x, cell.y);

        public int ToIndex(int x, int y) => y * Size + x;
        public int ToIndex(Vector2Int cell) => ToIndex(cell.x, cell.y);

        public Vector2Int ToCell(int index) => new(index % Size, index / Size);

        public override string ToString() => HasBoxes
            ? $"{Size}x{Size} ({BoxWidth}x{BoxHeight} boxes)"
            : $"{Size}x{Size}";
    }
}
