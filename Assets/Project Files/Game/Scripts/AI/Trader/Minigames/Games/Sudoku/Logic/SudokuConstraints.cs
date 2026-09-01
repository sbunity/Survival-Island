using System.Collections.Generic;

namespace Watermelon
{
    [System.Flags]
    public enum SudokuRuleFlags
    {
        None = 0,

        Row = 1 << 0,
        Column = 1 << 1,
        Box = 1 << 2
    }

    public interface ISudokuConstraint
    {
        string Caption { get; }

        bool IsSupported(SudokuLayout layout);

        void CollectUnits(SudokuLayout layout, List<int[]> units);
    }

    public class SudokuRowConstraint : ISudokuConstraint
    {
        public string Caption => "rows";

        public bool IsSupported(SudokuLayout layout) => true;

        public void CollectUnits(SudokuLayout layout, List<int[]> units)
        {
            for (var y = 0; y < layout.Size; y++)
            {
                var unit = new int[layout.Size];

                for (var x = 0; x < layout.Size; x++)
                    unit[x] = layout.ToIndex(x, y);

                units.Add(unit);
            }
        }
    }

    public class SudokuColumnConstraint : ISudokuConstraint
    {
        public string Caption => "columns";

        public bool IsSupported(SudokuLayout layout) => true;

        public void CollectUnits(SudokuLayout layout, List<int[]> units)
        {
            for (var x = 0; x < layout.Size; x++)
            {
                var unit = new int[layout.Size];

                for (var y = 0; y < layout.Size; y++)
                    unit[y] = layout.ToIndex(x, y);

                units.Add(unit);
            }
        }
    }

    public class SudokuBoxConstraint : ISudokuConstraint
    {
        public string Caption => "boxes";

        public bool IsSupported(SudokuLayout layout) => layout.HasBoxes;

        public void CollectUnits(SudokuLayout layout, List<int[]> units)
        {
            for (var originY = 0; originY < layout.Size; originY += layout.BoxHeight)
            {
                for (var originX = 0; originX < layout.Size; originX += layout.BoxWidth)
                {
                    var unit = new int[layout.Size];
                    var next = 0;

                    for (var y = 0; y < layout.BoxHeight; y++)
                    {
                        for (var x = 0; x < layout.BoxWidth; x++)
                            unit[next++] = layout.ToIndex(originX + x, originY + y);
                    }

                    units.Add(unit);
                }
            }
        }
    }

    public static class SudokuRuleFactory
    {
        public static List<ISudokuConstraint> Create(SudokuRuleFlags flags)
        {
            var constraints = new List<ISudokuConstraint>();

            if (flags.HasFlag(SudokuRuleFlags.Row))
                constraints.Add(new SudokuRowConstraint());

            if (flags.HasFlag(SudokuRuleFlags.Column))
                constraints.Add(new SudokuColumnConstraint());

            if (flags.HasFlag(SudokuRuleFlags.Box))
                constraints.Add(new SudokuBoxConstraint());

            return constraints;
        }
    }
}
