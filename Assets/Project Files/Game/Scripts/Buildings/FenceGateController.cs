using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [DisallowMultipleComponent]
    public class FenceGateController : MonoBehaviour, IBarrier
    {
        private const float SIDE_EPSILON = 0.001f;

        [SerializeField, HideInInspector] FencePath path;

        [BoxFoldout("Animation", "Animation")]
        [SerializeField] FenceGateAnimationType animationType = FenceGateAnimationType.Sink;

        [BoxFoldout("Animation", "Animation")]
        [ShowIf("EditorIsSinkSelected")]
        [SerializeField] FenceGateSinkAnimation sinkAnimation = new FenceGateSinkAnimation();

        [BoxFoldout("Animation", "Animation")]
        [ShowIf("EditorIsFadeSelected")]
        [SerializeField] FenceGateFadeAnimation fadeAnimation = new FenceGateFadeAnimation();

        [BoxFoldout("Animation", "Animation")]
        [ShowIf("EditorIsRotateSelected")]
        [SerializeField] FenceGateRotateAnimation rotateAnimation = new FenceGateRotateAnimation();

        [BoxFoldout("Opening", "Opening")]
        [SerializeField, Min(1)] int openingLogs = 4;
        [BoxFoldout("Opening", "Opening")]
        [SerializeField, Min(0f)] float openLeadMargin = 0.35f;
        [BoxFoldout("Opening", "Opening")]
        [SerializeField, Min(0f)] float occupancyWidth = 0.6f;
        [BoxFoldout("Opening", "Opening")]
        [SerializeField, Range(0f, 1f)] float centreDriftTolerance = 0.5f;

        [BoxFoldout("Closing", "Closing")]
        [SerializeField, Min(0f)] float closeDelay = 0.8f;
        [BoxFoldout("Closing", "Closing")]
        [SerializeField, Min(0.02f)] float pollInterval = 0.05f;

        [BoxFoldout("Debug", "Debug")]
        [SerializeField] bool drawGizmos = true;
        [BoxFoldout("Debug", "Debug")]
        [SerializeField, ReadOnly] int logsAmount;
        [BoxFoldout("Debug", "Debug")]
        [SerializeField, ReadOnly] int openingsAmount;

        private readonly List<Transform> logs = new();
        private readonly List<LogState> logStates = new();
        private readonly List<Opening> openings = new();

        private FenceGateAnimation activeAnimation;
        private float nextPollTime;
        private float logSpacing;
        private bool isInitialised;

        public FencePath Path => path;
        public float Length => path.TotalLength;
        public int LogCount => logs.Count;

        public float OpeningHalfWidth => Mathf.Clamp(openingLogs, 1, Mathf.Max(1, logs.Count)) * logSpacing * 0.5f;

        private float LeadSeconds => (activeAnimation != null ? activeAnimation.Duration : 0f) + openLeadMargin;

        private FenceGateAnimation ResolveAnimation()
        {
            return animationType switch
            {
                FenceGateAnimationType.Sink => sinkAnimation,
                FenceGateAnimationType.Fade => fadeAnimation,
                FenceGateAnimationType.Rotate => rotateAnimation,
                _ => null,
            };

        }

        private bool EditorIsSinkSelected()
        {
            return animationType == FenceGateAnimationType.Sink;
        }

        private bool EditorIsFadeSelected()
        {
            return animationType == FenceGateAnimationType.Fade;
        }

        private bool EditorIsRotateSelected()
        {
            return animationType == FenceGateAnimationType.Rotate;
        }

        private void OnEnable()
        {
            Initialise();
            nextPollTime = 0f;
        }

        private void OnDisable()
        {
            CloseEverything();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying || !isInitialised)
                return;

            var resolved = ResolveAnimation();

            if (ReferenceEquals(resolved, activeAnimation))
                return;

            CloseEverything();

            activeAnimation = resolved;

            activeAnimation?.Initialise(logs);
        }

        private void CloseEverything()
        {
            for (var i = 0; i < logStates.Count; i++)
            {
                logStates[i].IsOpen = false;
                logStates[i].IsCovered = false;
            }

            openings.Clear();
            openingsAmount = 0;

            activeAnimation?.SnapAllClosed();
        }

        private void Update()
        {
            if (Time.time < nextPollTime)
                return;

            nextPollTime = Time.time + pollInterval;

            Refresh();
        }

        private void Initialise()
        {
            logs.Clear();
            logStates.Clear();
            openings.Clear();

            isInitialised = false;
            logsAmount = 0;
            openingsAmount = 0;

            if (!path.IsValid)
                return;

            var found = new List<KeyValuePair<float, Transform>>();

            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                if (child.GetComponent<MeshRenderer>() == null)
                    continue;

                if (!path.Project(child.localPosition, out var alongPath, out _))
                    continue;

                found.Add(new KeyValuePair<float, Transform>(alongPath, child));
            }

            found.Sort((a, b) => a.Key.CompareTo(b.Key));

            for (var i = 0; i < found.Count; i++)
            {
                logs.Add(found[i].Value);
                logStates.Add(new LogState { AlongPath = found[i].Key });
            }

            logsAmount = logs.Count;
            isInitialised = logs.Count > 0;

            if (!isInitialised)
                return;

            logSpacing = logs.Count > 1
                ? (logStates[logs.Count - 1].AlongPath - logStates[0].AlongPath) / (logs.Count - 1)
                : path.TotalLength;

            activeAnimation = ResolveAnimation();

            activeAnimation?.Initialise(logs);
        }

        private void Refresh()
        {
            if (!isInitialised)
                return;

            UpdateOpenings();
            ApplyCoverage();
        }

        private void UpdateOpenings()
        {
            BarrierCrosserRegistry.RemoveInvalidCrossers();

            for (var i = 0; i < openings.Count; i++)
                openings[i].IsAlive = false;

            for (var i = 0; i < BarrierCrosserRegistry.Count; i++)
            {
                var crosser = BarrierCrosserRegistry.GetCrosser(i);

                if (crosser == null)
                    continue;

                var opening = FindOpening(crosser);

                if (TryGetIntendedCrossing(crosser, out var crossing))
                {
                    var centre = GetOpeningCentre(crossing);

                    if (opening == null)
                    {
                        openings.Add(new Opening { Owner = crosser, Centre = centre, Side = crossing.Side, IsAlive = true });
                        continue;
                    }

                    if (Mathf.Abs(opening.Centre - centre) > OpeningHalfWidth * centreDriftTolerance)
                        opening.Centre = centre;

                    if (Mathf.Abs(crossing.Side) > SIDE_EPSILON)
                        opening.Side = crossing.Side;

                    opening.IsAlive = true;
                    continue;
                }

                if (opening != null && IsInsideOpening(opening, crosser.Position))
                    opening.IsAlive = true;
            }

            for (var i = openings.Count - 1; i >= 0; i--)
            {
                if (!openings[i].IsAlive)
                    openings.RemoveAt(i);
            }

            openingsAmount = openings.Count;
        }

        private bool TryGetIntendedCrossing(IBarrierCrosser crosser, out BarrierCrossing crossing)
        {
            return crosser.TryGetCrossing(this, out crossing) && crossing.SecondsToReach <= LeadSeconds;
        }

        private float GetOpeningCentre(BarrierCrossing crossing)
        {
            return crossing.AlongPath + crossing.SlideDirection * OpeningHalfWidth;
        }

        private void ApplyCoverage()
        {
            for (var i = 0; i < logStates.Count; i++)
                logStates[i].IsCovered = false;

            for (var i = 0; i < openings.Count; i++)
            {
                GetOpeningRange(openings[i].Centre, out var first, out var count);

                for (var j = first; j < first + count; j++)
                {
                    logStates[j].IsCovered = true;
                    logStates[j].DistanceFromCentre = Mathf.Abs(logStates[j].AlongPath - openings[i].Centre);
                    logStates[j].Side = openings[i].Side;
                }
            }

            var now = Time.time;

            for (var i = 0; i < logStates.Count; i++)
            {
                var state = logStates[i];

                if (state.IsCovered)
                {
                    state.CloseAtTime = now + closeDelay;

                    if (state.IsOpen)
                        continue;

                    state.IsOpen = true;

                    activeAnimation?.SetLogOpen(i, true, new FenceLogOpenContext(state.DistanceFromCentre, state.Side));

                    continue;
                }

                if (!state.IsOpen || now < state.CloseAtTime)
                    continue;

                state.IsOpen = false;

                activeAnimation?.SetLogOpen(i, false, new FenceLogOpenContext(state.DistanceFromCentre, state.Side));
            }
        }

        private void GetOpeningRange(float centre, out int first, out int count)
        {
            count = Mathf.Clamp(openingLogs, 1, logs.Count);

            var insertion = 0;

            while (insertion < logStates.Count && logStates[insertion].AlongPath < centre)
                insertion++;

            first = Mathf.Clamp(insertion - count / 2, 0, logs.Count - count);
        }

        private Opening FindOpening(IBarrierCrosser crosser)
        {
            for (var i = 0; i < openings.Count; i++)
            {
                if (ReferenceEquals(openings[i].Owner, crosser))
                    return openings[i];
            }

            return null;
        }

        public bool IsLogOpen(int index)
        {
            return index >= 0 && index < logStates.Count && logStates[index].IsOpen;
        }

        #region IBarrier

        public bool Project(Vector3 worldPoint, out float alongPath, out float sideOffset)
        {
            return path.Project(transform.InverseTransformPoint(worldPoint), out alongPath, out sideOffset);
        }

        public Vector3 GetNormal(float alongPath)
        {
            return transform.TransformDirection(path.GetNormal(alongPath)).normalized;
        }

        public Vector3 GetPoint(float alongPath)
        {
            return transform.TransformPoint(path.SamplePosition(alongPath, 0f));
        }

        #endregion

        private bool IsInsideOpening(Opening opening, Vector3 worldPoint)
        {
            if (!Project(worldPoint, out var alongPath, out _))
                return false;

            if (Mathf.Abs(alongPath - opening.Centre) > OpeningHalfWidth)
                return false;

            return HorizontalDistanceToRun(worldPoint, alongPath) <= occupancyWidth;
        }

        private float HorizontalDistanceToRun(Vector3 worldPoint, float alongPath)
        {
            var onRun = GetPoint(alongPath);
            var offsetX = worldPoint.x - onRun.x;
            var offsetZ = worldPoint.z - onRun.z;

            return Mathf.Sqrt(offsetX * offsetX + offsetZ * offsetZ);
        }

#if UNITY_EDITOR
        public void SetPath(FencePath value)
        {
            path = value;

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        private void OnDrawGizmos()
        {
            if (!drawGizmos || !path.IsValid)
                return;

            Gizmos.color = new Color(1f, 0.6f, 0.1f);

            var steps = Mathf.Max(2, Mathf.CeilToInt(path.TotalLength));
            var previous = transform.TransformPoint(path.SamplePosition(0f, 0f));

            for (var i = 1; i <= steps; i++)
            {
                var next = transform.TransformPoint(path.SamplePosition(path.TotalLength * i / steps, 0f));

                Gizmos.DrawLine(previous, next);
                previous = next;
            }

            Gizmos.color = Color.green;

            for (var i = 0; i < logStates.Count; i++)
            {
                if (!logStates[i].IsOpen)
                    continue;

                var point = transform.TransformPoint(path.SamplePosition(logStates[i].AlongPath, 0f));
                Gizmos.DrawLine(point, point + Vector3.up * 2.5f);
            }

            Gizmos.color = Color.cyan;

            for (var i = 0; i < openings.Count; i++)
            {
                var point = transform.TransformPoint(path.SamplePosition(openings[i].Centre, 0f));
                Gizmos.DrawWireSphere(point + Vector3.up * 2.5f, 0.3f);
            }
        }

        private class LogState
        {
            public float AlongPath;
            public bool IsOpen;
            public bool IsCovered;
            public float CloseAtTime;
            public float DistanceFromCentre;
            public float Side;
        }

        private class Opening
        {
            public IBarrierCrosser Owner;
            public float Centre;
            public float Side;
            public bool IsAlive;
        }
    }
}
