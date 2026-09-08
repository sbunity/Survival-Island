using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class AttackAnchor
    {
        public BuildingBehavior Building { get; }
        public Vector3 Position { get; }
        public float LastThreatTime { get; private set; }

        public AttackAnchor(BuildingBehavior building, Vector3 position, float time)
        {
            Building = building;
            Position = position;
            LastThreatTime = time;
        }

        public void KeepAlive(float time)
        {
            LastThreatTime = time;
        }
    }

    public class AttackAnchorSet
    {
        private readonly List<AttackAnchor> anchors = new List<AttackAnchor>();

        public int Count => anchors.Count;
        public IReadOnlyList<AttackAnchor> Anchors => anchors;

        public void Register(BuildingBehavior building, Vector3 threatPosition, float mergeDistance, float time)
        {
            if (building == null)
                return;

            var position = building.GetAttackPosition(threatPosition);
            var mergeDistanceSqr = Mathf.Max(0f, mergeDistance) * Mathf.Max(0f, mergeDistance);

            for (var i = 0; i < anchors.Count; i++)
            {
                var anchor = anchors[i];

                if (anchor.Building != building || FlatDistanceSqr(anchor.Position, position) > mergeDistanceSqr)
                    continue;

                anchor.KeepAlive(time);
                return;
            }

            anchors.Add(new AttackAnchor(building, position, time));
        }

        public void RemoveExpired(float time, float cooldown)
        {
            for (var i = anchors.Count - 1; i >= 0; i--)
            {
                if (time >= anchors[i].LastThreatTime + cooldown)
                    anchors.RemoveAt(i);
            }
        }

        public bool IsInsideRadius(Vector3 position, float radius)
        {
            var radiusSqr = radius * radius;

            for (var i = 0; i < anchors.Count; i++)
            {
                if (FlatDistanceSqr(anchors[i].Position, position) <= radiusSqr)
                    return true;
            }

            return false;
        }

        public Vector3 GetNearest(Vector3 from, Vector3 fallback)
        {
            var nearest = fallback;
            var nearestDistanceSqr = float.MaxValue;

            for (var i = 0; i < anchors.Count; i++)
            {
                var distanceSqr = FlatDistanceSqr(anchors[i].Position, from);

                if (distanceSqr >= nearestDistanceSqr)
                    continue;

                nearestDistanceSqr = distanceSqr;
                nearest = anchors[i].Position;
            }

            return nearest;
        }

        public void Clear()
        {
            anchors.Clear();
        }

        private static float FlatDistanceSqr(Vector3 a, Vector3 b)
        {
            var offset = a - b;
            offset.y = 0f;

            return offset.sqrMagnitude;
        }
    }
}
