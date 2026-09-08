#pragma warning disable CS0414

using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

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
        private const string BAND_ROOT_NAME = "NavMesh Band";

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

        [BoxFoldout("Fence NavMesh", "Fence NavMesh")]
        [SerializeField] bool buildNavMeshBand = true;
        [BoxFoldout("Fence NavMesh", "Fence NavMesh")]
        [SerializeField] string navMeshAreaName = "Fence";
        [BoxFoldout("Fence NavMesh", "Fence NavMesh")]
        [SerializeField] string navMeshBandLayer = "Ground";
        [BoxFoldout("Fence NavMesh", "Fence NavMesh")]
        [SerializeField] float bandWidth = 0.5f;
        [BoxFoldout("Fence NavMesh", "Fence NavMesh")]
        [SerializeField] float bandBottomOffset = -0.96f;
        [BoxFoldout("Fence NavMesh", "Fence NavMesh")]
        [SerializeField] float bandTopOffset = 1f;
        [BoxFoldout("Fence NavMesh", "Fence NavMesh")]
        [SerializeField] float bandChunkLength = 2f;
        [BoxFoldout("Fence NavMesh", "Fence NavMesh")]
        [SerializeField] float bandExtensionStart;
        [BoxFoldout("Fence NavMesh", "Fence NavMesh")]
        [SerializeField] float bandExtensionEnd;
        [BoxFoldout("Fence NavMesh", "Fence NavMesh")]
        [SerializeField] bool autoBakeNavMesh = true;
        [BoxFoldout("Fence NavMesh", "Fence NavMesh")]
        [SerializeField, ReadOnly] int bandVolumes;

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

        private struct BandBox
        {
            public Vector3 Position;
            public float Yaw;
            public Vector3 Size;
        }

        private void Reset()
        {
            logsParent = transform.Find("Fence/Opened");

            CapturePath();

            if (TryGetLogs(out var logs))
            {
                logWidth = MeasureLogWidth(logs[0]);
                gap = pathLength / (logs.Count - 1) - logWidth;
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
            if (!TryGetLogs(out var logs))
                return;

            var first = logs[0];
            var last = logs[logs.Count - 1];

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

            CaptureHeightProfile(path, logs);
            WriteBackPathInfo(path);
        }

        [Button("Rebuild")]
        public void Rebuild()
        {
            if (Application.isPlaying)
                return;

            if (!TryGetLogs(out var logs))
                return;

            if (straightLength < 0f)
                CapturePath();

            if (!TryBuildPath(out var path))
                return;

            if (profileDistance == null || profileDistance.Length < 2)
                CaptureHeightProfile(path, logs);

            WarnIfPrefabInstance();

            var startPosition = logs[0].localPosition;
            var startRotation = logs[0].localRotation;
            var endPosition = logs[logs.Count - 1].localPosition;
            var endRotation = logs[logs.Count - 1].localRotation;

            logWidth = MeasureLogWidth(logs[0]);

            var length = path.TotalLength;
            var targetStep = Mathf.Max(logWidth + gap, MIN_STEP);
            var intervals = PickIntervals(length, targetStep);
            var wanted = intervals + 1;
            var step = length / intervals;

            Undo.RecordObject(this, "Rebuild Fence");

            ResizeLogs(logs, wanted);
            logs = CollectLogs();

            for (var i = 0; i < wanted; i++)
            {
                var distance = step * i;
                Sample(path, distance, out var x, out var z, out var yaw);

                var log = logs[i];

                Undo.RecordObject(log, "Rebuild Fence");
                Undo.RecordObject(log.gameObject, "Rebuild Fence");

                log.gameObject.name = "Fence Log " + (i + 1).ToString("D3");
                log.localPosition = new Vector3(x, HeightAt(distance), z);
                log.localRotation = Quaternion.Euler(0f, yaw, 0f);
            }

            logs[0].localPosition = startPosition;
            logs[0].localRotation = startRotation;

            logs[wanted - 1].localPosition = endPosition;
            logs[wanted - 1].localRotation = endRotation;

            RefreshAppearAnimation(logs);

            var bandChanged = ApplyBand(path);

            gap = step - logWidth;
            spacing = step;
            logsAmount = wanted;
            WriteBackPathInfo(path);

            EditorUtility.SetDirty(this);
            MarkDirty();

            if (bandChanged && autoBakeNavMesh)
                RequestNavMeshBake();
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

        private void ResizeLogs(List<Transform> logs, int wanted)
        {
            var band = FindBand();
            var template = logs[0];

            for (var i = logs.Count - 1; i >= wanted; i--)
            {
                Undo.DestroyObjectImmediate(logs[i].gameObject);
            }

            var current = Mathf.Min(logs.Count, wanted);

            while (current < wanted)
            {
                var copy = Instantiate(template.gameObject, logsParent);

                if (band != null && band.parent == logsParent)
                    copy.transform.SetSiblingIndex(band.GetSiblingIndex());
                else
                    copy.transform.SetAsLastSibling();

                Undo.RegisterCreatedObjectUndo(copy, "Rebuild Fence");
                current++;
            }
        }

        private bool TryBuildPath(out PathData path)
        {
            path = default;

            if (!TryGetLogs(out var logs))
                return false;

            var first = logs[0];
            var last = logs[logs.Count - 1];

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

        private void CaptureHeightProfile(PathData path, List<Transform> logs)
        {
            profileDistance = new float[logs.Count];
            profileHeight = new float[logs.Count];

            var walked = 0f;

            for (var i = 0; i < logs.Count; i++)
            {
                if (i > 0)
                {
                    var previous = logs[i - 1].localPosition;
                    var current = logs[i].localPosition;

                    walked += new Vector2(current.x - previous.x, current.z - previous.z).magnitude;
                }

                profileDistance[i] = walked;
                profileHeight[i] = logs[i].localPosition.y;
            }

            if (walked > MIN_STEP)
            {
                var scale = path.TotalLength / walked;

                for (var i = 0; i < logs.Count; i++)
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

        private void RefreshAppearAnimation(List<Transform> logs)
        {
            var appearAnimation = GetComponentInChildren<ScaleAnimationForUnlockable>(true);

            if (appearAnimation == null)
                return;

            var serializedAnimation = new SerializedObject(appearAnimation);
            var objectsToAppear = serializedAnimation.FindProperty("objectsToAppear");

            if (objectsToAppear == null)
                return;

            objectsToAppear.arraySize = logs.Count;

            for (var i = 0; i < logs.Count; i++)
            {
                objectsToAppear.GetArrayElementAtIndex(i).objectReferenceValue = logs[i];
            }

            serializedAnimation.ApplyModifiedProperties();
        }

        private void WriteBackPathInfo(PathData path)
        {
            pathLength = path.TotalLength;
            arcRadius = Mathf.Abs(path.Curvature) > CURVATURE_EPSILON ? 1f / path.Curvature : 0f;
            arcSweep = path.Curvature * path.ArcLength * Mathf.Rad2Deg;
        }

        private Transform FindBand()
        {
            return logsParent != null ? logsParent.Find(BAND_ROOT_NAME) : null;
        }

        private List<Transform> CollectLogs()
        {
            var logs = new List<Transform>();

            if (logsParent == null)
                return logs;

            for (var i = 0; i < logsParent.childCount; i++)
            {
                var child = logsParent.GetChild(i);

                if (child.name == BAND_ROOT_NAME)
                    continue;

                logs.Add(child);
            }

            return logs;
        }

        private bool TryGetLogs(out List<Transform> logs)
        {
            if (logsParent == null)
                logsParent = transform.Find("Fence/Opened");

            if (logsParent == null)
            {
                Debug.LogError("[Fence] Logs parent is not set and Fence/Opened was not found.", this);
                logs = null;
                return false;
            }

            logs = CollectLogs();

            if (logs.Count < 2)
            {
                Debug.LogError("[Fence] Needs at least two logs to work with.", this);
                return false;
            }

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

        private List<BandBox> BuildBandBoxes(PathData path)
        {
            var boxes = new List<BandBox>();
            var start = -Mathf.Max(0f, bandExtensionStart);
            var length = path.TotalLength + Mathf.Max(0f, bandExtensionEnd) - start;
            var chunks = Mathf.Max(1, Mathf.CeilToInt(length / Mathf.Max(0.25f, bandChunkLength)));
            var chunkLength = length / chunks;

            for (var i = 0; i < chunks; i++)
            {
                var from = start + chunkLength * i;
                var to = start + chunkLength * (i + 1);
                var middle = (from + to) * 0.5f;

                Sample(path, from, out var fromX, out var fromZ, out _);
                Sample(path, to, out var toX, out var toZ, out _);
                Sample(path, middle, out var midX, out var midZ, out var yaw);

                var chord = new Vector2(toX - fromX, toZ - fromZ).magnitude;
                var height = HeightAt(middle);
                var bottom = height + bandBottomOffset;
                var top = height + bandTopOffset;

                boxes.Add(new BandBox
                {
                    Position = new Vector3(midX, (bottom + top) * 0.5f, midZ),
                    Yaw = yaw,
                    Size = new Vector3(chord * 1.25f, Mathf.Max(0.05f, top - bottom), bandWidth)
                });
            }

            return boxes;
        }

        private bool ApplyBand(PathData path)
        {
            var band = FindBand();

            if (!buildNavMeshBand)
            {
                bandVolumes = 0;

                if (band == null)
                    return false;

                Undo.DestroyObjectImmediate(band.gameObject);
                return true;
            }

            var area = NavMesh.GetAreaFromName(navMeshAreaName);

            if (area < 0)
            {
                Debug.LogError("[Fence] NavMesh area '" + navMeshAreaName + "' does not exist. Add it in Navigation > Areas.", this);
                return false;
            }

            var layer = LayerMask.NameToLayer(navMeshBandLayer);

            if (layer < 0)
            {
                Debug.LogError("[Fence] Layer '" + navMeshBandLayer + "' does not exist.", this);
                return false;
            }

            var boxes = BuildBandBoxes(path);
            var changed = false;

            if (band == null)
            {
                var holder = new GameObject(BAND_ROOT_NAME);
                holder.transform.SetParent(logsParent, false);
                holder.transform.SetAsLastSibling();

                Undo.RegisterCreatedObjectUndo(holder, "Rebuild Fence");
                band = holder.transform;
                changed = true;
            }

            while (band.childCount < boxes.Count)
            {
                var volumeObject = new GameObject(BAND_ROOT_NAME);
                volumeObject.transform.SetParent(band, false);
                volumeObject.AddComponent<NavMeshModifierVolume>();

                Undo.RegisterCreatedObjectUndo(volumeObject, "Rebuild Fence");
                changed = true;
            }

            for (var i = band.childCount - 1; i >= boxes.Count; i--)
            {
                Undo.DestroyObjectImmediate(band.GetChild(i).gameObject);
                changed = true;
            }

            for (var i = 0; i < boxes.Count; i++)
            {
                var child = band.GetChild(i);
                var volume = child.GetComponent<NavMeshModifierVolume>();

                if (volume == null)
                {
                    volume = Undo.AddComponent<NavMeshModifierVolume>(child.gameObject);
                    changed = true;
                }

                var box = boxes[i];
                var rotation = Quaternion.Euler(0f, box.Yaw, 0f);

                child.gameObject.name = "NavMesh Band " + (i + 1).ToString("D2");

                if (child.gameObject.layer != layer)
                {
                    Undo.RecordObject(child.gameObject, "Rebuild Fence");
                    child.gameObject.layer = layer;
                    changed = true;
                }

                if ((child.localPosition - box.Position).sqrMagnitude > 0.000001f ||
                    Quaternion.Angle(child.localRotation, rotation) > 0.001f)
                {
                    Undo.RecordObject(child, "Rebuild Fence");
                    child.localPosition = box.Position;
                    child.localRotation = rotation;
                    changed = true;
                }

                if ((volume.size - box.Size).sqrMagnitude > 0.000001f || volume.area != area ||
                    volume.center != Vector3.zero)
                {
                    Undo.RecordObject(volume, "Rebuild Fence");
                    volume.size = box.Size;
                    volume.center = Vector3.zero;
                    volume.area = area;
                    changed = true;
                }
            }

            bandVolumes = boxes.Count;

            return changed;
        }

        [Button("Bake NavMesh")]
        public void RequestNavMeshBake()
        {
            if (Application.isPlaying)
                return;

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                Debug.Log("[Fence] NavMesh band updated. Save the prefab, then bake the World NavMesh from the scene.", this);
                return;
            }

            var surface = GetComponentInParent<NavMeshSurface>(true);

            if (surface == null)
            {
                Debug.LogWarning("[Fence] No NavMeshSurface above the fence - nothing to bake.", this);
                return;
            }

            if (surface.navMeshData == null)
            {
                Debug.LogWarning("[Fence] '" + surface.name + "' has no baked data yet. Bake it once from its own inspector.", this);
                return;
            }

            var data = surface.navMeshData;
            var operation = surface.UpdateNavMesh(data);

            EditorApplication.CallbackFunction onUpdate = null;
            onUpdate = () =>
            {
                if (operation != null && !operation.isDone)
                    return;

                EditorApplication.update -= onUpdate;

                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();

                Debug.Log("[Fence] NavMesh rebaked on '" + surface.name + "'.", surface);
            };

            EditorApplication.update += onUpdate;
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
