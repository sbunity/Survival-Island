using UnityEngine;

namespace Watermelon
{
    public enum SudokuPlacement
    {
        Rejected = 0,

        Correct = 1,

        Wrong = 2
    }

    public class SudokuBoard
    {
        public const int EMPTY = -1;

        public SudokuRuleSet Rules { get; }
        public SudokuLayout Layout => Rules.Layout;

        private readonly int[] cells;
        private readonly int[] solution;
        private readonly bool[] givens;

        public int Size => Layout.Size;
        public int SymbolCount => Layout.SymbolCount;

        public int EmptyCount { get; private set; }

        public bool IsSolved => EmptyCount <= 0;

        public SudokuBoard(SudokuRuleSet rules, int[] solution, int[] puzzle)
        {
            Rules = rules;

            this.solution = solution;

            cells = puzzle;
            givens = new bool[cells.Length];

            for (var i = 0; i < cells.Length; i++)
            {
                givens[i] = cells[i] != EMPTY;

                if (!givens[i])
                    EmptyCount++;
            }
        }

        public int Get(int x, int y) => Layout.IsInside(x, y) ? cells[Layout.ToIndex(x, y)] : EMPTY;
        public int Get(Vector2Int cell) => Get(cell.x, cell.y);

        public int GetSolution(Vector2Int cell) => Layout.IsInside(cell) ? solution[Layout.ToIndex(cell)] : EMPTY;

        public bool IsGiven(Vector2Int cell) => Layout.IsInside(cell) && givens[Layout.ToIndex(cell)];

        public bool IsEmpty(Vector2Int cell) => Get(cell) == EMPTY;

        public int CountRemaining(int symbol)
        {
            var total = 0;

            for (var i = 0; i < cells.Length; i++)
            {
                if (cells[i] == EMPTY && solution[i] == symbol)
                    total++;
            }

            return total;
        }

        public SudokuPlacement Place(Vector2Int cell, int symbol)
        {
            if (!Layout.IsInside(cell) || symbol < 0 || symbol >= SymbolCount)
                return SudokuPlacement.Rejected;

            var index = Layout.ToIndex(cell);

            if (cells[index] != EMPTY)
                return SudokuPlacement.Rejected;

            if (solution[index] != symbol)
                return SudokuPlacement.Wrong;

            cells[index] = symbol;
            EmptyCount--;

            return SudokuPlacement.Correct;
        }

        public bool TryGetConflict(Vector2Int cell, int symbol, out Vector2Int conflict)
        {
            conflict = Vector2Int.zero;

            if (!Layout.IsInside(cell))
                return false;

            if (!Rules.TryGetConflict(cells, Layout.ToIndex(cell), symbol, out var conflictIndex))
                return false;

            conflict = Layout.ToCell(conflictIndex);

            return true;
        }
    }
}
