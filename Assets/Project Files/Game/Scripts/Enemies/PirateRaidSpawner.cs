using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class PirateRaidSpawner : RaidSpawner
    {
        [BoxFoldout("Models", label: "Models")]
        [SerializeField] List<PlayerGraphics> models = new();

        [BoxFoldout("Sea", label: "Sea Spawn Line")]
        [SerializeField] Transform seaLineStart;
        [BoxFoldout("Sea")]
        [SerializeField] Transform seaLineEnd;

        [BoxFoldout("Landing", label: "Landing Line")]
        [SerializeField] Transform landingLineStart;
        [BoxFoldout("Landing")]
        [SerializeField] Transform landingLineEnd;

        [BoxFoldout("Wave")]
        [SerializeField, Min(0.1f)] float navMeshSampleDistance = 3f;

        public override Vector3 Center
        {
            get
            {
                if (landingLineStart != null && landingLineEnd != null)
                    return Vector3.Lerp(landingLineStart.position, landingLineEnd.position, 0.5f);

                return transform.position;
            }
        }

        public override int MaxWaveSize
        {
            get
            {
                var count = 0;

                for (var i = 0; i < models.Count; i++)
                {
                    if (models[i] != null && models.IndexOf(models[i]) == i)
                        count++;
                }

                return count;
            }
        }

        private readonly List<PlayerGraphics> waveModels = new();

        private RaidPointLine seaPoints;
        private RaidPointLine landingPoints;

        protected override bool PrepareWave(int count)
        {
            if (seaLineStart == null || seaLineEnd == null || landingLineStart == null || landingLineEnd == null)
            {
                Debug.LogError("[Pirate Raid Spawner] Assign the start and end transforms of both the Sea Spawn Line and the Landing Line.", this);
                return false;
            }

            CollectDistinctModels(waveModels);

            if (waveModels.Count == 0)
            {
                Debug.LogError("[Pirate Raid Spawner] The Models list is empty.", this);
                return false;
            }

            waveModels.Shuffle();

            if (waveModels.Count > count)
                waveModels.RemoveRange(count, waveModels.Count - count);

            seaPoints ??= new RaidPointLine(transform, "Pirate Sea Point");
            landingPoints ??= new RaidPointLine(transform, "Pirate Landing Point");

            if (!seaPoints.Layout(seaLineStart.position, seaLineEnd.position, count, navMeshSampleDistance, NavMeshAreas.Water))
                Debug.LogWarning("[Pirate Raid Spawner] Part of the Sea Spawn Line is not over water.", this);

            if (!landingPoints.Layout(landingLineStart.position, landingLineEnd.position, count, navMeshSampleDistance, NavMeshAreas.Land))
                Debug.LogWarning("[Pirate Raid Spawner] Part of the Landing Line is not over land.", this);

            for (var i = 0; i < count; i++)
                landingPoints.Face(i, 2f * landingPoints[i].position - seaPoints[i].position, transform.rotation);

            return true;
        }

        protected override bool SpawnEnemy(BaseEnemyBehavior enemy, int index)
        {
            if (enemy is not PirateEnemyBehavior pirate)
            {
                Debug.LogError($"[Pirate Raid Spawner] Enemy type {enemyType} is not a pirate.", this);
                return false;
            }

            pirate.SetModel(waveModels[index]);
            pirate.Spawn(seaPoints[index].position, landingPoints[index]);

            return true;
        }

        private void CollectDistinctModels(List<PlayerGraphics> result)
        {
            result.Clear();

            for (var i = 0; i < models.Count; i++)
            {
                if (models[i] != null && !result.Contains(models[i]))
                    result.Add(models[i]);
            }
        }

        private void Reset()
        {
            enemyType = EnemyType.Pirate;
        }

        private void OnDrawGizmosSelected()
        {
            var previewCount = Mathf.Max(1, Mathf.Min(EnemyCount, MaxWaveSize));

            RaidPointLine.DrawGizmo(seaLineStart, seaLineEnd, previewCount, Color.cyan);
            RaidPointLine.DrawGizmo(landingLineStart, landingLineEnd, previewCount, Color.red);

            if (seaLineStart == null || seaLineEnd == null || landingLineStart == null || landingLineEnd == null)
                return;

            Gizmos.color = Color.yellow;

            for (var i = 0; i < previewCount; i++)
            {
                Gizmos.DrawLine(
                    RaidPointLine.Evaluate(seaLineStart.position, seaLineEnd.position, i, previewCount),
                    RaidPointLine.Evaluate(landingLineStart.position, landingLineEnd.position, i, previewCount));
            }
        }
    }
}
