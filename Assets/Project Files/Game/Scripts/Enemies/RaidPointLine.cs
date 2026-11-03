using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public sealed class RaidPointLine
    {
        private readonly Transform parent;
        private readonly string pointName;
        private readonly List<Transform> points = new();

        public Transform this[int index] => points[index];

        public RaidPointLine(Transform parent, string pointName)
        {
            this.parent = parent;
            this.pointName = pointName;
        }

        public bool Layout(Vector3 start, Vector3 end, int count, float sampleDistance, int areaMask)
        {
            Resize(count);

            var allSnapped = true;

            for (var i = 0; i < count; i++)
            {
                var position = Evaluate(start, end, i, count);

                if (NavMesh.SamplePosition(position, out NavMeshHit hit, sampleDistance, areaMask))
                    position = hit.position;
                else
                    allSnapped = false;

                points[i].position = position;
            }

            return allSnapped;
        }

        public void Face(int index, Vector3 target, Quaternion fallback)
        {
            var point = points[index];

            point.rotation = FlatLookRotation(point.position, target, fallback);
        }

        public static Vector3 Evaluate(Vector3 start, Vector3 end, int index, int count)
        {
            var t = count <= 1 ? 0.5f : (float)index / (count - 1);

            return Vector3.Lerp(start, end, t);
        }

        public static Quaternion FlatLookRotation(Vector3 from, Vector3 to, Quaternion fallback)
        {
            var direction = to - from;
            direction.y = 0f;

            return direction.sqrMagnitude > Mathf.Epsilon ? Quaternion.LookRotation(direction.normalized) : fallback;
        }

        public static void DrawGizmo(Transform start, Transform end, int count, Color color)
        {
            if (start == null || end == null)
                return;

            Gizmos.color = color;
            Gizmos.DrawLine(start.position, end.position);
            Gizmos.DrawWireSphere(start.position, 0.4f);
            Gizmos.DrawWireSphere(end.position, 0.4f);

            for (var i = 0; i < count; i++)
                Gizmos.DrawWireSphere(Evaluate(start.position, end.position, i, count), 0.25f);
        }

        private void Resize(int count)
        {
            for (var i = points.Count - 1; i >= count; i--)
            {
                if (points[i] != null)
                    Object.Destroy(points[i].gameObject);

                points.RemoveAt(i);
            }

            while (points.Count < count)
            {
                var pointObject = new GameObject($"{pointName} {points.Count}");
                pointObject.transform.SetParent(parent);

                points.Add(pointObject.transform);
            }
        }
    }
}
