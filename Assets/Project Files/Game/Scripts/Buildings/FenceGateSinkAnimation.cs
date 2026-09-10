using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [Serializable]
    public class FenceGateSinkAnimation : FenceGateAnimation
    {
        [SerializeField, Min(0f)] float sinkDepth;
        [SerializeField, Min(0f)] float depthMargin = 0.15f;

        [Space]
        [SerializeField, Min(0.05f)] float openDuration = 0.25f;
        [SerializeField, Min(0.05f)] float closeDuration = 0.3f;
        [SerializeField, Min(0f)] float staggerPerMetre = 0.05f;

        [Space]
        [SerializeField] Ease.Type openEasing = Ease.Type.CubicIn;
        [SerializeField] Ease.Type closeEasing = Ease.Type.BackOut;

        private IReadOnlyList<Transform> logs;
        private float[] closedHeights;
        private TweenCase[] tweens;
        private float appliedDepth;

        public float AppliedDepth => appliedDepth;

        public override float Duration => openDuration;

        public override void Initialise(IReadOnlyList<Transform> logs)
        {
            this.logs = logs;

            closedHeights = new float[logs.Count];
            tweens = new TweenCase[logs.Count];

            var tallest = 0f;

            for (var i = 0; i < logs.Count; i++)
            {
                if (logs[i] == null)
                    continue;

                closedHeights[i] = logs[i].localPosition.y;

                var logRenderer = logs[i].GetComponent<Renderer>();

                if (logRenderer != null)
                    tallest = Mathf.Max(tallest, logRenderer.bounds.size.y);
            }

            appliedDepth = sinkDepth > 0f ? sinkDepth : tallest + depthMargin;
        }

        public override void SetLogOpen(int logIndex, bool isOpen, in FenceLogOpenContext context)
        {
            if (logs == null || logIndex < 0 || logIndex >= logs.Count)
                return;

            var log = logs[logIndex];

            if (log == null)
                return;

            tweens[logIndex].KillActive();

            var target = isOpen ? closedHeights[logIndex] - appliedDepth : closedHeights[logIndex];
            var duration = isOpen ? openDuration : closeDuration;
            var easing = isOpen ? openEasing : closeEasing;
            var delay = staggerPerMetre * Mathf.Max(0f, context.DistanceFromCentre);

            tweens[logIndex] = log.DOLocalMoveY(target, duration, delay).SetEasing(easing);
        }

        public override void SnapAllClosed()
        {
            if (logs == null)
                return;

            tweens.KillActive();

            for (var i = 0; i < logs.Count; i++)
            {
                if (logs[i] == null)
                    continue;

                var position = logs[i].localPosition;
                position.y = closedHeights[i];

                logs[i].localPosition = position;
            }
        }
    }
}
