using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class Match3Objective
    {
        public int TileId { get; }
        public int Required { get; }

        public int Collected { get; private set; }

        public bool IsComplete => Collected >= Required;

        public Match3Objective(int tileId, int required)
        {
            TileId = tileId;
            Required = Mathf.Max(1, required);
        }

        public void Report(List<Match3Clear> cleared)
        {
            if (cleared == null)
                return;

            for (var i = 0; i < cleared.Count; i++)
            {
                if (cleared[i].TileId == TileId)
                    Collected++;
            }
        }

        public void Report(Match3Resolution resolution)
        {
            if (resolution == null)
                return;

            for (var i = 0; i < resolution.Steps.Count; i++)
                Report(resolution.Steps[i].Cleared);
        }
    }
}
