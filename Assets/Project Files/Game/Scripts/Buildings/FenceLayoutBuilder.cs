#pragma warning disable CS0414

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Watermelon
{
    public class FenceLayoutBuilder : MonoBehaviour
    {
        private const float NOT_CAPTURED = -1f;
        private const float CURVATURE_EPSILON = 0.000001f;
        private const float MIN_STEP = 0.01f;
        private const int MAX_LOGS = 1000;

        [BoxFoldout("Fence Layout", "Fence Layout")]
        [SerializeField, Delayed] float gap = 0.05f;
        [BoxFoldout("Fence Layout", "Fence Layout")]
        [SerializeField] bool rebuildOnChange = true;
        [BoxFoldout("Fence Layout", "Fence Layout")]
        [SerializeField, ReadOnly] int logsAmount;
        [BoxFoldout("Fence Layout", "Fence Layout")]
        [SerializeField, ReadOnly] float logWidth;
        [BoxFoldout("Fence Layout", "Fence Layout")]
        [SerializeField, ReadOnly] float spacing;

        [BoxFoldout("Fence Path", "Fence Path")]
        [SerializeField] Transform logsParent;
        [BoxFoldout("Fence Path", "Fence Path")]
        [SerializeField] float straightLength = NOT_CAPTURED;
        [BoxFoldout("Fence Path", "Fence Path")]
        [SerializeField, ReadOnly] float pathLength;
        [BoxFoldout("Fence Path", "Fence Path")]
        [SerializeField, ReadOnly] float arcRadius;
        [BoxFoldout("Fence Path", "Fence Path")]
        [SerializeField, ReadOnly] float arcSweep;

        [SerializeField, HideInInspector] float[] profileDistance;
        [SerializeField, HideInInspector] float[] profileHeight;

#if UNITY_EDITOR
        private bool rebuildScheduled;

        private struct PathData
        {
            public float StartX;
            public float StartZ;
            public float Straight;
            public float Curvature;
            public float ArcLength;

            public float TotalLength => Straight + ArcLength;
        }

        private void Reset()
        {
            logsParent = transform.Find("Fence/Opened");

            CapturePath();

            if (TryGetLogs(out var first, out _, out var count))
            {
                logWidth = MeasureLogWidth(first);
                gap = pathLength / (count - 1) - logWidth;
            }
        }

        private void OnValidate()
        {
            if (!rebuildOnChange || Application.isPlaying || rebuildScheduled)
                return;

            if (EditorUtility.IsPersistent(this))
                return;

            rebuildScheduled = true;

            EditorApplication.delayCall += () =>
            {
                rebuildScheduled = false;

                if (this == null || Application.isPlaying)
                    return;

                Rebuild();
            };
        }

        [Button("Capture path from logs")]
        public void CapturePath()
        {
            if (!TryGetLogs(out var first, out var last, out var count))
                return;

            var sweep = Mathf.DeltaAngle(0f, last.localEulerAngles.y) * Mathf.Deg2Rad;
            var deltaZ = last.localPosition.z - first.localPosition.z;

            if (Mathf.Abs(Mathf.Cos(sweep) - 1f) < CURVATURE_EPSILON)
            {
                straightLength = last.localPosition.x - first.localPosition.x;
            }
            else
            {
                var radius = deltaZ / (Mathf.Cos(sweep) - 1f);
                straightLength = last.localPosition.x - radius * Mathf.Sin(sweep) - first.localPosition.x;
            }

            straightLength = Mathf.Max(0f, straightLength);

            if (!TryBuildPath(out var path))
                return;

            CaptureHeightProfile(path, count);
            WriteBackPathInfo(path);
        }

        [Button("Rebuild")]
        public void Rebuild()
        {
            if (Application.isPlaying)
                return;

            if (!TryGetLogs(out var first, out var last, out var count))
                return;

            if (straightLength < 0f)
                CapturePath();

            if (!TryBuildPath(out var path))
                return;

            if (profileDistance == null || profileDistance.Length < 2)
                CaptureHeightProfile(path, count);

            WarnIfPrefabInstance();

            var startPosition = first.localPosition;
            var startRotation = first.localRotation;
            var endPosition = last.localPosition;
            var endRotation = last.localRotation;

            logWidth = MeasureLogWidth(first);

            var length = path.TotalLength;
            var targetStep = Mathf.Max(logWidth + gap, MIN_STEP);
            var intervals = PickIntervals(length, targetStep);
            var wanted = intervals + 1;
            var step = length / intervals;

            Undo.RecordObject(this, "Rebuild Fence");

            ResizeLogs(wanted, first);

            for (var i = 0; i < wanted; i++)
            {
                var distance = step * i;
                Sample(path, distance, out var x, out var z, out var yaw);

                var log = logsParent.GetChild(i);

                Undo.RecordObject(log, "Rebuild Fence");
                Undo.RecordObject(log.gameObject, "Rebuild Fence");

                log.gameObject.name = "Fence Log " + (i + 1).ToString("D3");
                log.localPosition = new Vector3(x, HeightAt(distance), z);
                log.localRotation = Quaternion.Euler(0f, yaw, 0f);
            }

            var newFirst = logsParent.GetChild(0);
            newFirst.localPosition = startPosition;
            newFirst.localRotation = startRotation;

            var newLast = logsParent.GetChild(wanted - 1);
            newLast.localPosition = endPosition;
            newLast.localRotation = endRotation;

            RefreshAppearAnimation();

            gap = step - logWidth;
            spacing = step;
            logsAmount = wanted;
            WriteBackPathInfo(path);

            EditorUtility.SetDirty(this);
            MarkDirty();
        }

        private int PickIntervals(float length, float targetStep)
        {
            var lower = Mathf.Max(1, Mathf.FloorToInt(length / targetStep));
            var upper = lower + 1;

            var intervals = Mathf.Abs(length / lower - targetStep) <= Mathf.Abs(length / upper - targetStep)
                ? lower
                : upper;

            if (intervals + 1 > MAX_LOGS)
            {
                Debug.LogWarning("[Fence] Gap " + gap + " would need " + (intervals + 1) + " logs, clamped to " + MAX_LOGS + ".", this);
                intervals = MAX_LOGS - 1;
            }

            return intervals;
        }

        private void ResizeLogs(int wanted, Transform template)
        {
            while (logsParent.childCount < wanted)
            {
                var copy = Instantiate(template.gameObject, logsParent);
                copy.transform.SetAsLastSibling();

                Undo.RegisterCreatedObjectUndo(copy, "Rebuild Fence");
            }

            for (var i = logsParent.childCount - 1; i >= wanted; i--)
            {
                Undo.DestroyObjectImmediate(logsParent.GetChild(i).gameObject);
            }
        }

        private bool TryBuildPath(out PathData path)
        {
            path = default;

            if (!TryGetLogs(out var first, out var last, out _))
                return false;

            path.StartX = first.localPosition.x;
            path.StartZ = first.localPosition.z;
            path.Straight = straightLength;

            var deltaX = last.localPosition.x - (path.StartX + path.Straight);
            var deltaZ = last.localPosition.z - path.StartZ;
            var chordSqr = deltaX * deltaX + deltaZ * deltaZ;

            if (chordSqr < CURVATURE_EPSILON)
                return false;

            path.Curvature = -2f * deltaZ / chordSqr;

            if (Mathf.Abs(path.Curvature) < CURVATURE_EPSILON)
            {
                path.Curvature = 0f;
                path.ArcLength = deltaX;
            }
            else
            {
                var theta = Mathf.Atan2(path.Curvature * deltaX, 1f + path.Curvature * deltaZ);

                if (theta * path.Curvature < 0f)
                    theta += Mathf.Sign(path.Curvature) * 2f * Mathf.PI;

                path.ArcLength = theta / path.Curvature;
            }

            return path.TotalLength > MIN_STEP;
        }

        private static void Sample(PathData path, float distance, out float x, out float z, out float yawDegrees)
        {
            if (distance <= path.Straight || path.Curvature == 0f)
            {
                x = path.StartX + distance;
                z = path.StartZ;
                yawDegrees = 0f;
                return;
            }

            var theta = path.Curvature * (distance - path.Straight);

            x = path.StartX + path.Straight + Mathf.Sin(theta) / path.Curvature;
            z = path.StartZ + (Mathf.Cos(theta) - 1f) / path.Curvature;
            yawDegrees = theta * Mathf.Rad2Deg;
        }

        private void CaptureHeightProfile(PathData path, int count)
        {
            profileDistance = new float[count];
            profileHeight = new float[count];

            var walked = 0f;

            for (var i = 0; i < count; i++)
            {
                var log = logsParent.GetChild(i);

                if (i > 0)
                {
                    var previous = logsParent.GetChild(i - 1).localPosition;
                    var current = log.localPosition;

                    walked += new Vector2(current.x - previous.x, current.z - previous.z).magnitude;
                }

                profileDistance[i] = walked;
                profileHeight[i] = log.localPosition.y;
            }

            if (walked > MIN_STEP)
            {
                var scale = path.TotalLength / walked;

                for (var i = 0; i < count; i++)
                {
                    profileDistance[i] *= scale;
                }
            }
        }

        private float HeightAt(float distance)
        {
            if (profileDistance == null || profileDistance.Length < 2)
                return 0f;

            if (distance <= profileDistance[0])
                return profileHeight[0];

            var last = profileDistance.Length - 1;

            if (distance >= profileDistance[last])
                return profileHeight[last];

            for (var i = 1; i <= last; i++)
            {
                if (distance > profileDistance[i])
                    continue;

                var span = profileDistance[i] - profileDistance[i - 1];
                var t = span > 0f ? (distance - profileDistance[i - 1]) / span : 0f;

                return Mathf.Lerp(profileHeight[i - 1], profileHeight[i], t);
            }

            return profileHeight[last];
        }

        private void RefreshAppearAnimation()
        {
            var appearAnimation = GetComponentInChildren<ScaleAnimationForUnlockable>(true);

            if (appearAnimation == null)
                return;

            var serializedAnimation = new SerializedObject(appearAnimation);
            var objectsToAppear = serializedAnimation.FindProperty("objectsToAppear");

            if (objectsToAppear == null)
                return;

            objectsToAppear.arraySize = logsParent.childCount;

            for (var i = 0; i < logsParent.childCount; i++)
            {
                objectsToAppear.GetArrayElementAtIndex(i).objectReferenceValue = logsParent.GetChild(i);
            }

            serializedAnimation.ApplyModifiedProperties();
        }

        private void WriteBackPathInfo(PathData path)
        {
            pathLength = path.TotalLength;
            arcRadius = Mathf.Abs(path.Curvature) > CURVATURE_EPSILON ? 1f / path.Curvature : 0f;
            arcSweep = path.Curvature * path.ArcLength * Mathf.Rad2Deg;
        }

        private bool TryGetLogs(out Transform first, out Transform last, out int count)
        {
            first = null;
            last = null;
            count = 0;

            if (logsParent == null)
                logsParent = transform.Find("Fence/Opened");

            if (logsParent == null)
            {
                Debug.LogError("[Fence] Logs parent is not set and Fence/Opened was not found.", this);
                return false;
            }

            count = logsParent.childCount;

            if (count < 2)
            {
                Debug.LogError("[Fence] Needs at least two logs to work with.", this);
                return false;
            }

            first = logsParent.GetChild(0);
            last = logsParent.GetChild(count - 1);

            return true;
        }

        private static float MeasureLogWidth(Transform log)
        {
            var filter = log.GetComponent<MeshFilter>();

            if (filter != null && filter.sharedMesh != null)
                return filter.sharedMesh.bounds.size.x * Mathf.Abs(log.localScale.x);

            var meshRenderer = log.GetComponent<Renderer>();

            return meshRenderer != null ? meshRenderer.bounds.size.x : 0f;
        }

        private void WarnIfPrefabInstance()
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                return;

            if (!PrefabUtility.IsPartOfPrefabInstance(gameObject))
                return;

            Debug.LogWarning("[Fence] Rebuilding a prefab instance in a scene - the new layout will be stored as scene overrides. Open the prefab and rebuild there instead.", this);
        }

        private void MarkDirty()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage != null)
            {
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                return;
            }

            if (gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}
