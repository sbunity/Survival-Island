using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [Serializable]
    public class FenceGateRotateAnimation : FenceGateAnimation
    {
        private const float RESTING_ANGLE = 0.5f;
        private const float SIDE_EPSILON = 0.001f;
        private const float DEFAULT_SIDE = 1f;

        [SerializeField, Range(10f, 170f)] float liftAngle = 95f;
        [SerializeField] float pivotOffset;

        [Space]
        [SerializeField, Min(0.05f)] float openDuration = 0.3f;
        [SerializeField, Min(0.05f)] float closeDuration = 0.35f;
        [SerializeField, Min(0f)] float staggerPerMetre = 0.05f;

        [Space]
        [SerializeField] Ease.Type openEasing = Ease.Type.CubicOut;
        [SerializeField] Ease.Type closeEasing = Ease.Type.CubicIn;

        private IReadOnlyList<Transform> logs;
        private LogHinge[] hinges;
        private float[] angles;
        private float[] sides;
        private TweenCase[] tweens;

        public override float Duration => openDuration;

        public override void Initialise(IReadOnlyList<Transform> logs)
        {
            this.logs = logs;

            hinges = new LogHinge[logs.Count];
            angles = new float[logs.Count];
            sides = new float[logs.Count];
            tweens = new TweenCase[logs.Count];

            for (var i = 0; i < logs.Count; i++)
            {
                sides[i] = DEFAULT_SIDE;

                if (logs[i] == null)
                    continue;

                hinges[i] = LogHinge.Measure(logs[i], pivotOffset);
            }
        }

        public override void SetLogOpen(int logIndex, bool isOpen, in FenceLogOpenContext context)
        {
            if (logs == null || logIndex < 0 || logIndex >= logs.Count)
                return;

            if (logs[logIndex] == null)
                return;

            tweens[logIndex].KillActive();

            if (isOpen)
                sides[logIndex] = ResolveSide(logIndex, context.CrossingSide);

            var index = logIndex;
            var target = isOpen ? liftAngle : 0f;
            var duration = isOpen ? openDuration : closeDuration;
            var easing = isOpen ? openEasing : closeEasing;
            var delay = staggerPerMetre * Mathf.Max(0f, context.DistanceFromCentre);

            tweens[logIndex] = this.DOAction<float>(
                (from, to, progress) => ApplyAngle(index, Mathf.Lerp(from, to, progress)),
                angles[logIndex],
                target,
                duration,
                delay).SetEasing(easing);
        }

        public override void SnapAllClosed()
        {
            if (logs == null)
                return;

            tweens.KillActive();

            for (var i = 0; i < logs.Count; i++)
                ApplyAngle(i, 0f);
        }

        private float ResolveSide(int index, float requestedSide)
        {
            var currentSide = sides[index] != 0f ? sides[index] : DEFAULT_SIDE;

            if (Mathf.Abs(angles[index]) > RESTING_ANGLE)
                return currentSide;

            if (Mathf.Abs(requestedSide) < SIDE_EPSILON)
                return currentSide;

            return Mathf.Sign(requestedSide);
        }

        private void ApplyAngle(int index, float angle)
        {
            angles[index] = angle;

            var log = logs[index];

            if (log == null)
                return;

            hinges[index].Apply(log, -sides[index] * angle);
        }

        private readonly struct LogHinge
        {
            private readonly Vector3 closedPosition;
            private readonly Quaternion closedRotation;
            private readonly Vector3 axis;
            private readonly Vector3 arm;

            private LogHinge(Vector3 closedPosition, Quaternion closedRotation, Vector3 axis, Vector3 arm)
            {
                this.closedPosition = closedPosition;
                this.closedRotation = closedRotation;
                this.axis = axis;
                this.arm = arm;
            }

            public static LogHinge Measure(Transform log, float pivotOffset)
            {
                var rotation = log.localRotation;

                return new LogHinge(
                    log.localPosition,
                    rotation,
                    rotation * Vector3.right,
                    rotation * (Vector3.up * (MeasureTopOffset(log) + pivotOffset)));
            }

            public void Apply(Transform log, float angle)
            {
                var rotation = Quaternion.AngleAxis(angle, axis);

                log.localRotation = rotation * closedRotation;
                log.localPosition = closedPosition + arm - rotation * arm;
            }

            private static float MeasureTopOffset(Transform log)
            {
                var filter = log.GetComponent<MeshFilter>();

                if (filter != null && filter.sharedMesh != null)
                {
                    var bounds = filter.sharedMesh.bounds;

                    return (bounds.center.y + bounds.extents.y) * log.localScale.y;
                }

                var logRenderer = log.GetComponent<Renderer>();

                return logRenderer != null ? logRenderer.bounds.max.y - log.position.y : 0f;
            }
        }
    }
}
