using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public struct Match3Clear
    {
        public Vector2Int Cell;
        public int TileId;
    }

    public struct Match3Move
    {
        public Vector2Int From;
        public Vector2Int To;
    }

    public struct Match3Spawn
    {
        public Vector2Int Cell;
        public int TileId;
    }

    public delegate void Match3StepCallback(Match3Step step);

    public class Match3Step
    {
        public readonly List<Match3Clear> Cleared = new();
        public readonly List<Match3Move> Moves = new();
        public readonly List<Match3Spawn> Spawns = new();

        public bool IsEmpty => Cleared.Count == 0 && Moves.Count == 0 && Spawns.Count == 0;
    }

    public class Match3Resolution
    {
        public bool IsValid;

        public Vector2Int From;
        public Vector2Int To;

        public readonly List<Match3Step> Steps = new();

        public int CountCleared(int tileId)
        {
            var total = 0;

            for (var i = 0; i < Steps.Count; i++)
            {
                var cleared = Steps[i].Cleared;
                for (var j = 0; j < cleared.Count; j++)
                {
                    if (cleared[j].TileId == tileId)
                        total++;
                }
            }

            return total;
        }
    }
}
