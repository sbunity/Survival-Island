using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [DisallowMultipleComponent]
    public class FenceGateController : MonoBehaviour, IBarrier
    {
        private const float MIN_SECTION_LENGTH = 0.5f;

        [SerializeField, HideInInspector] FencePath path;

        [BoxFoldout("Sections", "Sections")]
        [SerializeField, Min(MIN_SECTION_LENGTH)] float sectionLength = 2f;
        [BoxFoldout("Sections", "Sections")]
        [SerializeField, Min(0f)] float boundaryMargin = 0.4f;
        [BoxFoldout("Sections", "Sections")]
        [SerializeField, ReadOnly] int sectionsAmount;

        [BoxFoldout("Opening", "Opening")]
        [SerializeField, Min(0f)] float openLeadSeconds = 0.5f;
        [BoxFoldout("Opening", "Opening")]
        [SerializeField, Min(0f)] float maxOpenDistance = 3f;
        [BoxFoldout("Opening", "Opening")]
        [SerializeField, Min(0f)] float occupancyWidth = 0.9f;
        [BoxFoldout("Opening", "Opening")]
        [SerializeField, Min(0f)] float closeDelay = 1.2f;
        [BoxFoldout("Opening", "Opening")]
        [SerializeField, Min(0.02f)] float pollInterval = 0.1f;

        [BoxFoldout("Debug", "Debug")]
        [SerializeField] bool drawGizmos = true;

        private readonly List<SectionState> sections = new List<SectionState>();
        private float nextPollTime;

        public FencePath Path => path;

        public int SectionCount => GetSectionCountForLength();
        public float SectionSpan => SectionCount > 0 ? path.TotalLength / SectionCount : 0f;

        public float Length => path.TotalLength;

        private void OnEnable()
        {
            EnsureSections();
            nextPollTime = 0f;
        }

        private void OnDisable()
        {
            for (var i = 0; i < sections.Count; i++)
            {
                if (!sections[i].IsOpen)
                    continue;

                sections[i].IsOpen = false;
                OnSectionClosed(i, true);
            }
        }

        private void Update()
        {
            if (Time.time < nextPollTime)
                return;

            nextPollTime = Time.time + pollInterval;

            Refresh();
        }

        private void Refresh()
        {
            EnsureSections();

            if (sections.Count == 0)
                return;

            for (var i = 0; i < sections.Count; i++)
                sections[i].IsWanted = false;

            BarrierCrosserRegistry.RemoveInvalidCrossers();

            for (var i = 0; i < BarrierCrosserRegistry.Count; i++)
            {
                var crosser = BarrierCrosserRegistry.GetCrosser(i);

                if (crosser == null)
                    continue;

                MarkOccupied(crosser);
                MarkApproaching(crosser);
            }

            ApplyWantedState();
        }

        private void MarkOccupied(IBarrierCrosser crosser)
        {
            if (!TryProjectNearRun(crosser.Position, out var alongPath, out var sideOffset))
                return;

            if (Mathf.Abs(sideOffset) > occupancyWidth)
                return;

            WantSectionAt(alongPath);
        }

        private void MarkApproaching(IBarrierCrosser crosser)
        {
            if (!crosser.TryGetCrossing(this, out var crossing))
                return;

            if (crossing.SecondsToReach > openLeadSeconds)
                return;

            WantSectionAt(crossing.AlongPath);
        }

        private void WantSectionAt(float alongPath)
        {
            var index = GetSectionIndex(alongPath);

            if (index < 0 || index >= sections.Count)
                return;

            sections[index].IsWanted = true;

            var span = SectionSpan;
            var offsetInSection = alongPath - index * span;

            if (offsetInSection < boundaryMargin && index > 0)
                sections[index - 1].IsWanted = true;

            if (span - offsetInSection < boundaryMargin && index < sections.Count - 1)
                sections[index + 1].IsWanted = true;
        }

        private void ApplyWantedState()
        {
            for (var i = 0; i < sections.Count; i++)
            {
                var section = sections[i];

                if (section.IsWanted)
                {
                    section.CloseAtTime = Time.time + closeDelay;

                    if (section.IsOpen)
                        continue;

                    section.IsOpen = true;
                    OnSectionOpened(i);

                    continue;
                }

                if (!section.IsOpen || Time.time < section.CloseAtTime)
                    continue;

                section.IsOpen = false;
                OnSectionClosed(i, false);
            }
        }

        protected virtual void OnSectionOpened(int index)
        {
            // Anim()
        }

        protected virtual void OnSectionClosed(int index, bool immediately)
        {
            // Anim()
        }

        public bool IsSectionOpen(int index)
        {
            return index >= 0 && index < sections.Count && sections[index].IsOpen;
        }

        public int GetSectionIndex(float alongPath)
        {
            var count = SectionCount;
            var span = SectionSpan;

            if (count == 0 || span <= 0f)
                return -1;

            return Mathf.Clamp(Mathf.FloorToInt(alongPath / span), 0, count - 1);
        }

        public bool TryGetSectionCentre(int index, out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;

            if (index < 0 || index >= sections.Count)
                return false;

            var span = SectionSpan;
            var local = path.SamplePosition((index + 0.5f) * span, 0f);

            worldPosition = transform.TransformPoint(local);
            return true;
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

        #endregion

        private bool TryProjectNearRun(Vector3 worldPoint, out float alongPath, out float sideOffset)
        {
            alongPath = 0f;
            sideOffset = 0f;

            var local = transform.InverseTransformPoint(worldPoint);

            if (!path.Project(local, out alongPath, out sideOffset))
                return false;

            var onPath = path.SamplePosition(alongPath, 0f);
            var offsetX = local.x - onPath.x;
            var offsetZ = local.z - onPath.z;

            return offsetX * offsetX + offsetZ * offsetZ <= maxOpenDistance * maxOpenDistance;
        }

        private void EnsureSections()
        {
            var count = GetSectionCountForLength();

            if (sections.Count == count)
                return;

            sections.Clear();

            for (var i = 0; i < count; i++)
                sections.Add(new SectionState());

            sectionsAmount = count;
        }

        private int GetSectionCountForLength()
        {
            if (!path.IsValid)
                return 0;

            return Mathf.Max(1, Mathf.CeilToInt(path.TotalLength / Mathf.Max(MIN_SECTION_LENGTH, sectionLength)));
        }

#if UNITY_EDITOR
        public void SetPath(FencePath value)
        {
            path = value;
            sectionsAmount = GetSectionCountForLength();

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        private void OnDrawGizmos()
        {
            if (!drawGizmos || !path.IsValid)
                return;

            var count = SectionCount;
            var span = SectionSpan;

            for (var i = 0; i < count; i++)
            {
                Gizmos.color = IsSectionOpen(i) ? Color.green : new Color(1f, 0.6f, 0.1f);

                var from = transform.TransformPoint(path.SamplePosition(i * span, 0f));
                var to = transform.TransformPoint(path.SamplePosition((i + 1) * span, 0f));

                Gizmos.DrawLine(from + Vector3.up * 2f, to + Vector3.up * 2f);
                Gizmos.DrawLine(from, from + Vector3.up * 2f);
            }

            var end = transform.TransformPoint(path.SamplePosition(path.TotalLength, 0f));
            Gizmos.DrawLine(end, end + Vector3.up * 2f);
        }

        private class SectionState
        {
            public bool IsOpen;
            public bool IsWanted;
            public float CloseAtTime;
        }
    }
}
