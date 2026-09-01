namespace Watermelon
{
    public static class SudokuSolver
    {
        public const int EMPTY = SudokuBoard.EMPTY;

        public const int FILL_NODE_BUDGET = 500000;
        public const int UNIQUENESS_NODE_BUDGET = 50000;

        private const int BUDGET_EXCEEDED = -1;

        public static bool TryFill(int[] cells, SudokuRuleSet rules, System.Random random, int nodeBudget = FILL_NODE_BUDGET)
        {
            if (cells == null || rules == null)
                return false;

            return new Search(cells, rules, random, true, nodeBudget).Run(1) == 1;
        }

        public static bool TryCountSolutions(int[] cells, SudokuRuleSet rules, int limit, int nodeBudget, out int solutions)
        {
            solutions = 0;

            if (cells == null || rules == null)
                return false;

            var found = new Search(cells, rules, null, false, nodeBudget).Run(limit);

            if (found < 0)
                return false;

            solutions = found;

            return true;
        }

        private class Search
        {
            private readonly int[] cells;
            private readonly SudokuRuleSet rules;
            private readonly System.Random random;
            private readonly bool keepSolution;
            private readonly int nodeBudget;
            private readonly int symbolCount;
            private readonly int fullMask;

            private readonly int[][] orders;

            private int nodes;

            public Search(int[] cells, SudokuRuleSet rules, System.Random random, bool keepSolution, int nodeBudget)
            {
                this.cells = cells;
                this.rules = rules;
                this.random = random;
                this.keepSolution = keepSolution;
                this.nodeBudget = nodeBudget;

                symbolCount = rules.Layout.SymbolCount;
                fullMask = symbolCount >= 31 ? int.MaxValue : (1 << symbolCount) - 1;

                orders = new int[cells.Length + 1][];

                for (var i = 0; i < orders.Length; i++)
                    orders[i] = new int[symbolCount];
            }

            public int Run(int limit) => Run(limit, 0);

            private int Run(int limit, int depth)
            {
                if (++nodes > nodeBudget)
                    return BUDGET_EXCEEDED;

                var bestIndex = -1;
                var bestMask = 0;
                var bestCount = int.MaxValue;

                for (var i = 0; i < cells.Length; i++)
                {
                    if (cells[i] != EMPTY)
                        continue;

                    var mask = GetCandidates(i);
                    var count = CountBits(mask);

                    if (count == 0)
                        return 0;

                    if (count >= bestCount)
                        continue;

                    bestIndex = i;
                    bestMask = mask;
                    bestCount = count;

                    if (count == 1)
                        break;
                }

                if (bestIndex < 0)
                    return 1;

                var order = orders[depth];
                var optionCount = FillOrder(order, bestMask);

                var found = 0;

                for (var i = 0; i < optionCount; i++)
                {
                    cells[bestIndex] = order[i];

                    var result = Run(limit - found, depth + 1);

                    if (result == BUDGET_EXCEEDED)
                    {
                        cells[bestIndex] = EMPTY;

                        return BUDGET_EXCEEDED;
                    }

                    found += result;

                    if (found >= limit)
                    {
                        if (!keepSolution)
                            cells[bestIndex] = EMPTY;

                        return found;
                    }

                    cells[bestIndex] = EMPTY;
                }

                return found;
            }

            private int GetCandidates(int index)
            {
                var mask = fullMask;
                var peers = rules.GetPeers(index);

                for (var i = 0; i < peers.Length; i++)
                {
                    var symbol = cells[peers[i]];

                    if (symbol >= 0)
                        mask &= ~(1 << symbol);
                }

                return mask;
            }

            private int FillOrder(int[] order, int mask)
            {
                var count = 0;

                for (var symbol = 0; symbol < symbolCount; symbol++)
                {
                    if ((mask & (1 << symbol)) != 0)
                        order[count++] = symbol;
                }

                if (random != null)
                {
                    for (var i = count - 1; i > 0; i--)
                    {
                        var j = random.Next(0, i + 1);

                        (order[i], order[j]) = (order[j], order[i]);
                    }
                }

                return count;
            }

            private static int CountBits(int mask)
            {
                var count = 0;

                while (mask != 0)
                {
                    mask &= mask - 1;

                    count++;
                }

                return count;
            }
        }
    }
}
