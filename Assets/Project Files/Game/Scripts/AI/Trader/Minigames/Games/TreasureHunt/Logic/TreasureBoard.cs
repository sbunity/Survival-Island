using UnityEngine;

namespace Watermelon
{
    public class TreasureBoard
    {
        public const int MIN_SIZE = 2;

        private readonly bool[] dug;

        public int Columns { get; }
        public int Rows { get; }

        public int CellCount => Columns * Rows;

        public Vector2Int Treasure { get; }

        public int DigsTotal { get; }
        public int DigsUsed { get; private set; }

        public int DigsLeft => Mathf.Max(0, DigsTotal - DigsUsed);

        public bool IsFound { get; private set; }

        public bool IsOutOfDigs => DigsLeft <= 0;

        public TreasureBoard(int columns, int rows, Vector2Int treasure, int digs)
        {
            Columns = Mathf.Max(MIN_SIZE, columns);
            Rows = Mathf.Max(MIN_SIZE, rows);

            Treasure = new Vector2Int(Mathf.Clamp(treasure.x, 0, Columns - 1), Mathf.Clamp(treasure.y, 0, Rows - 1));

            DigsTotal = Mathf.Clamp(digs, 1, CellCount);

            dug = new bool[CellCount];
        }

        public bool IsInside(Vector2Int cell) => cell.x >= 0 && cell.x < Columns && cell.y >= 0 && cell.y < Rows;

        public int ToIndex(Vector2Int cell) => cell.y * Columns + cell.x;

        public Vector2Int ToCell(int index) => new(index % Columns, index / Columns);

        public bool IsDug(Vector2Int cell) => IsInside(cell) && dug[ToIndex(cell)];

        public bool IsTreasure(Vector2Int cell) => cell == Treasure;

        public bool CanDig(Vector2Int cell) => !IsFound && !IsOutOfDigs && IsInside(cell) && !IsDug(cell);

        public bool Dig(Vector2Int cell)
        {
            if (!CanDig(cell))
                return false;

            dug[ToIndex(cell)] = true;
            DigsUsed++;

            IsFound = IsTreasure(cell);

            return true;
        }

        public int DistanceTo(Vector2Int cell, TreasureDistanceMode mode) => TreasureDistance.Measure(cell, Treasure, mode);

        public static Vector2Int RollTreasure(int columns, int rows, System.Random random)
        {
            return new Vector2Int(random.Next(0, Mathf.Max(1, columns)), random.Next(0, Mathf.Max(1, rows)));
        }
    }
}
