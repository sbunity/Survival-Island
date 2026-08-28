using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class Match3Board
    {
        public const int EMPTY = -1;
        public const int MIN_MATCH_LENGTH = 3;

        private const int SHUFFLE_ATTEMPTS = 64;

        public int Columns { get; }
        public int Rows { get; }
        public int TileTypeCount { get; }

        private readonly int[] cells;
        private readonly System.Random random;

        private readonly HashSet<Vector2Int> matchBuffer = new HashSet<Vector2Int>();
        private readonly List<Vector2Int> dirtyBuffer = new List<Vector2Int>();

        public Match3Board(int columns, int rows, int tileTypeCount, System.Random random)
        {
            Columns = Mathf.Max(MIN_MATCH_LENGTH, columns);
            Rows = Mathf.Max(MIN_MATCH_LENGTH, rows);
            TileTypeCount = Mathf.Max(MIN_MATCH_LENGTH, tileTypeCount);

            this.random = random;

            cells = new int[Columns * Rows];
        }

        public bool IsInside(int x, int y) => x >= 0 && x < Columns && y >= 0 && y < Rows;
        public bool IsInside(Vector2Int cell) => IsInside(cell.x, cell.y);

        public int Get(int x, int y) => IsInside(x, y) ? cells[y * Columns + x] : EMPTY;
        public int Get(Vector2Int cell) => Get(cell.x, cell.y);

        public void Set(int x, int y, int tileId)
        {
            if (IsInside(x, y))
                cells[y * Columns + x] = tileId;
        }

        public void Set(Vector2Int cell, int tileId) => Set(cell.x, cell.y, tileId);

        public static bool AreAdjacent(Vector2Int a, Vector2Int b)
        {
            var delta = a - b;

            return Mathf.Abs(delta.x) + Mathf.Abs(delta.y) == 1;
        }

        public void Fill()
        {
            for (var y = 0; y < Rows; y++)
            {
                for (var x = 0; x < Columns; x++)
                    Set(x, y, RollTileWithoutMatch(x, y));
            }

            EnsurePlayable();
        }

        public Match3Resolution Swap(Vector2Int from, Vector2Int to)
        {
            var resolution = new Match3Resolution { From = from, To = to };

            if (!IsInside(from) || !IsInside(to) || !AreAdjacent(from, to))
                return resolution;

            SwapCells(from, to);

            dirtyBuffer.Clear();
            dirtyBuffer.Add(from);
            dirtyBuffer.Add(to);

            CollectMatches(dirtyBuffer, matchBuffer);

            if (matchBuffer.Count == 0)
            {
                SwapCells(from, to);

                return resolution;
            }

            resolution.IsValid = true;

            while (matchBuffer.Count > 0)
            {
                var step = new Match3Step();

                foreach (var cell in matchBuffer)
                {
                    step.Cleared.Add(new Match3Clear { Cell = cell, TileId = Get(cell) });
                    Set(cell, EMPTY);
                }

                ApplyGravity(step);

                resolution.Steps.Add(step);

                dirtyBuffer.Clear();
                for (var i = 0; i < step.Moves.Count; i++)
                    dirtyBuffer.Add(step.Moves[i].To);
                for (var i = 0; i < step.Spawns.Count; i++)
                    dirtyBuffer.Add(step.Spawns[i].Cell);

                CollectMatches(dirtyBuffer, matchBuffer);
            }

            return resolution;
        }

        public bool HasAvailableMove()
        {
            for (var y = 0; y < Rows; y++)
            {
                for (var x = 0; x < Columns; x++)
                {
                    var cell = new Vector2Int(x, y);

                    if (x + 1 < Columns && CreatesMatch(cell, new Vector2Int(x + 1, y)))
                        return true;

                    if (y + 1 < Rows && CreatesMatch(cell, new Vector2Int(x, y + 1)))
                        return true;
                }
            }

            return false;
        }

        public void Shuffle()
        {
            var tiles = new List<int>(cells.Length);
            for (var i = 0; i < cells.Length; i++)
                tiles.Add(cells[i]);

            for (var attempt = 0; attempt < SHUFFLE_ATTEMPTS; attempt++)
            {
                random.Shuffle(tiles);

                for (var i = 0; i < cells.Length; i++)
                    cells[i] = tiles[i];

                if (!HasAnyMatch() && HasAvailableMove())
                    return;
            }

            Fill();
        }

        private void EnsurePlayable()
        {
            for (var attempt = 0; attempt < SHUFFLE_ATTEMPTS; attempt++)
            {
                if (HasAvailableMove())
                    return;

                for (var y = 0; y < Rows; y++)
                {
                    for (var x = 0; x < Columns; x++)
                        Set(x, y, RollTileWithoutMatch(x, y));
                }
            }
        }

        private int RollTileWithoutMatch(int x, int y)
        {
            var forbiddenHorizontal = Get(x - 1, y) != EMPTY && Get(x - 1, y) == Get(x - 2, y) ? Get(x - 1, y) : EMPTY;
            var forbiddenVertical = Get(x, y - 1) != EMPTY && Get(x, y - 1) == Get(x, y - 2) ? Get(x, y - 1) : EMPTY;

            for (var attempt = 0; attempt < SHUFFLE_ATTEMPTS; attempt++)
            {
                var tileId = random.Next(0, TileTypeCount);

                if (tileId != forbiddenHorizontal && tileId != forbiddenVertical)
                    return tileId;
            }

            return random.Next(0, TileTypeCount);
        }

        private void SwapCells(Vector2Int a, Vector2Int b)
        {
            var temp = Get(a);
            Set(a, Get(b));
            Set(b, temp);
        }

        private bool CreatesMatch(Vector2Int a, Vector2Int b)
        {
            if (Get(a) == Get(b))
                return false;

            SwapCells(a, b);

            var found = HasMatchAt(a) || HasMatchAt(b);

            SwapCells(a, b);

            return found;
        }

        private void ApplyGravity(Match3Step step)
        {
            for (var x = 0; x < Columns; x++)
            {
                var write = Rows - 1;

                for (var y = Rows - 1; y >= 0; y--)
                {
                    if (Get(x, y) == EMPTY)
                        continue;

                    if (write != y)
                    {
                        step.Moves.Add(new Match3Move
                        {
                            From = new Vector2Int(x, y),
                            To = new Vector2Int(x, write)
                        });

                        Set(x, write, Get(x, y));
                        Set(x, y, EMPTY);
                    }

                    write--;
                }

                for (var y = write; y >= 0; y--)
                {
                    var tileId = random.Next(0, TileTypeCount);

                    Set(x, y, tileId);

                    step.Spawns.Add(new Match3Spawn
                    {
                        Cell = new Vector2Int(x, y),
                        TileId = tileId
                    });
                }
            }
        }

        private void CollectMatches(List<Vector2Int> dirtyCells, HashSet<Vector2Int> result)
        {
            result.Clear();

            for (var i = 0; i < dirtyCells.Count; i++)
                CollectMatchesAt(dirtyCells[i], result);
        }

        private void CollectMatchesAt(Vector2Int cell, HashSet<Vector2Int> result)
        {
            var tileId = Get(cell);
            if (tileId == EMPTY)
                return;

            var left = cell.x;
            while (Get(left - 1, cell.y) == tileId) left--;

            var right = cell.x;
            while (Get(right + 1, cell.y) == tileId) right++;

            if (right - left + 1 >= MIN_MATCH_LENGTH)
            {
                for (var x = left; x <= right; x++)
                    result.Add(new Vector2Int(x, cell.y));
            }

            var top = cell.y;
            while (Get(cell.x, top - 1) == tileId) top--;

            var bottom = cell.y;
            while (Get(cell.x, bottom + 1) == tileId) bottom++;

            if (bottom - top + 1 >= MIN_MATCH_LENGTH)
            {
                for (var y = top; y <= bottom; y++)
                    result.Add(new Vector2Int(cell.x, y));
            }
        }

        private bool HasMatchAt(Vector2Int cell)
        {
            var tileId = Get(cell);
            if (tileId == EMPTY)
                return false;

            var horizontal = 1;
            for (var x = cell.x - 1; Get(x, cell.y) == tileId; x--) horizontal++;
            for (var x = cell.x + 1; Get(x, cell.y) == tileId; x++) horizontal++;

            if (horizontal >= MIN_MATCH_LENGTH)
                return true;

            var vertical = 1;
            for (var y = cell.y - 1; Get(cell.x, y) == tileId; y--) vertical++;
            for (var y = cell.y + 1; Get(cell.x, y) == tileId; y++) vertical++;

            return vertical >= MIN_MATCH_LENGTH;
        }

        private bool HasAnyMatch()
        {
            for (var y = 0; y < Rows; y++)
            {
                for (var x = 0; x < Columns; x++)
                {
                    if (HasMatchAt(new Vector2Int(x, y)))
                        return true;
                }
            }

            return false;
        }
    }
}
