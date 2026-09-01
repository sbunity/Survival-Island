using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Watermelon
{
    public class SudokuRuleSet
    {
        private static readonly int[] NO_PEERS = new int[0];

        public SudokuLayout Layout { get; }

        private readonly List<int[]> units = new List<int[]>();
        private readonly List<string> captions = new List<string>();
        private readonly int[][] peers;

        public IReadOnlyList<int[]> Units => units;
        public IReadOnlyList<string> Captions => captions;

        public bool IsEmpty => units.Count == 0;

        public SudokuRuleSet(SudokuLayout layout, IEnumerable<ISudokuConstraint> constraints)
        {
            Layout = layout;

            if (constraints != null)
            {
                foreach (var constraint in constraints)
                {
                    if (constraint == null)
                        continue;

                    if (!constraint.IsSupported(layout))
                    {
                        Debug.LogWarning($"[Sudoku]: the \"{constraint.Caption}\" rule does not fit a {layout} board and was skipped.");

                        continue;
                    }

                    constraint.CollectUnits(layout, units);

                    if (!captions.Contains(constraint.Caption))
                        captions.Add(constraint.Caption);
                }
            }

            peers = BuildPeers();
        }

        public int[] GetPeers(int index) => index >= 0 && index < peers.Length ? peers[index] : NO_PEERS;

        public bool IsLegal(int[] cells, int index, int symbol)
        {
            return !TryGetConflict(cells, index, symbol, out _);
        }

        public bool TryGetConflict(int[] cells, int index, int symbol, out int conflictIndex)
        {
            conflictIndex = -1;

            if (cells == null || symbol < 0)
                return false;

            var cellPeers = GetPeers(index);

            for (var i = 0; i < cellPeers.Length; i++)
            {
                if (cells[cellPeers[i]] != symbol)
                    continue;

                conflictIndex = cellPeers[i];

                return true;
            }

            return false;
        }

        public string Describe()
        {
            if (captions.Count == 0)
                return string.Empty;

            var builder = new StringBuilder();

            for (var i = 0; i < captions.Count; i++)
            {
                if (i > 0)
                    builder.Append(i == captions.Count - 1 ? " and " : ", ");

                builder.Append(captions[i]);
            }

            return builder.ToString();
        }

        private int[][] BuildPeers()
        {
            var sets = new HashSet<int>[Layout.CellCount];

            for (var i = 0; i < sets.Length; i++)
                sets[i] = new HashSet<int>();

            for (var u = 0; u < units.Count; u++)
            {
                var unit = units[u];

                for (var i = 0; i < unit.Length; i++)
                {
                    for (var j = 0; j < unit.Length; j++)
                    {
                        if (i != j)
                            sets[unit[i]].Add(unit[j]);
                    }
                }
            }

            var result = new int[sets.Length][];

            for (var i = 0; i < sets.Length; i++)
            {
                result[i] = new int[sets[i].Count];
                sets[i].CopyTo(result[i]);
            }

            return result;
        }
    }
}
