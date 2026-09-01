using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class SudokuGenerator
    {
        public static SudokuBoard Generate(SudokuRuleSet rules, int holes, System.Random random)
        {
            if (rules == null || random == null)
                return null;

            if (rules.IsEmpty)
            {
                Debug.LogError("[Sudoku]: the difficulty has no rules switched on, so there is nothing to solve.");

                return null;
            }

            var solution = new int[rules.Layout.CellCount];

            for (var i = 0; i < solution.Length; i++)
                solution[i] = SudokuBoard.EMPTY;

            if (!SudokuSolver.TryFill(solution, rules, random))
            {
                Debug.LogError($"[Sudoku]: no {rules.Layout} grid satisfies the chosen rules.");

                return null;
            }

            var puzzle = new int[solution.Length];
            System.Array.Copy(solution, puzzle, solution.Length);

            var order = new List<int>(puzzle.Length);

            for (var i = 0; i < puzzle.Length; i++)
                order.Add(i);

            random.Shuffle(order);

            var target = Mathf.Clamp(holes, 1, puzzle.Length - 1);
            var dug = 0;

            for (var i = 0; i < order.Count && dug < target; i++)
            {
                var index = order[i];
                var symbol = puzzle[index];

                puzzle[index] = SudokuBoard.EMPTY;

                if (SudokuSolver.TryCountSolutions(puzzle, rules, 2, SudokuSolver.UNIQUENESS_NODE_BUDGET, out var solutions) && solutions == 1)
                {
                    dug++;

                    continue;
                }

                puzzle[index] = symbol;
            }

            return new SudokuBoard(rules, solution, puzzle);
        }
    }
}
