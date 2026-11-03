using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class SkeletonRaidSpawner : RaidSpawner
    {
        [BoxFoldout("Spawn Line", label: "Spawn Line")]
        [SerializeField] Transform lineStart;
        [BoxFoldout("Spawn Line")]
        [SerializeField] Transform lineEnd;
        [BoxFoldout("Spawn Line")]
        [Tooltip("Optional. Spawned enemies face this transform (e.g. the base center). Leave empty to keep the line orientation.")]
        [SerializeField] Transform facingTarget;

        [BoxFoldout("Wave")]
        [Tooltip("How far each line point is allowed to snap onto the baked NavMesh.")]
        [SerializeField, Min(0.1f)] float navMeshSampleDistance = 3f;

        public override Vector3 Center
        {
            get
            {
                if (lineStart != null && lineEnd != null)
                    return Vector3.Lerp(lineStart.position, lineEnd.position, 0.5f);

                if (lineStart != null)
                    return lineStart.position;

                return transform.position;
            }
        }

        private RaidPointLine spawnPoints;

        protected override bool PrepareWave(int count)
        {
            if (lineStart == null || lineEnd == null)
            {
                Debug.LogError("[Skeleton Raid Spawner] Assign both Line Start and Line End transforms.", this);
                return false;
            }

            spawnPoints ??= new RaidPointLine(transform, "Raid Spawn Point");
            spawnPoints.Layout(lineStart.position, lineEnd.position, count, navMeshSampleDistance, NavMesh.AllAreas);

            for (var i = 0; i < count; i++)
            {
                if (facingTarget != null)
                    spawnPoints.Face(i, facingTarget.position, spawnPoints[i].rotation);
                else
                    spawnPoints[i].rotation = transform.rotation;
            }

            return true;
        }

        protected override bool SpawnEnemy(BaseEnemyBehavior enemy, int index)
        {
            enemy.Spawn(spawnPoints[index]);

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            RaidPointLine.DrawGizmo(lineStart, lineEnd, EnemyCount, Color.red);
        }
    }
}
