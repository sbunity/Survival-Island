using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class SkeletonRaidSpawner : MonoBehaviour
    {
        [SerializeField] EnemyType enemyType = EnemyType.Skeleton;

        [BoxFoldout("Spawn Line", label: "Spawn Line")]
        [SerializeField] Transform lineStart;
        [BoxFoldout("Spawn Line")]
        [SerializeField] Transform lineEnd;
        [BoxFoldout("Spawn Line")]
        [Tooltip("Optional. Spawned enemies face this transform (e.g. the base center). Leave empty to keep the line orientation.")]
        [SerializeField] Transform facingTarget;

        [BoxFoldout("Wave", label: "Wave")]
        [SerializeField, Min(1)] int enemyCount = 10;
        [BoxFoldout("Wave")]
        [Tooltip("How far each line point is allowed to snap onto the baked NavMesh.")]
        [SerializeField, Min(0.1f)] float navMeshSampleDistance = 3f;

        public bool IsActive { get; private set; }
        public int AliveCount => aliveEnemies.Count;
        public int TotalSpawned { get; private set; }

        public Vector3 Center
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

        public event SimpleCallback EnemyDefeated;
        public event SimpleCallback WaveCleared;

        private readonly List<BaseEnemyBehavior> aliveEnemies = new List<BaseEnemyBehavior>();
        private readonly Dictionary<BaseEnemyBehavior, SimpleCallback> deathHandlers = new Dictionary<BaseEnemyBehavior, SimpleCallback>();
        private readonly List<Transform> spawnPoints = new List<Transform>();

        public bool SpawnWave()
        {
            StopWave();

            if (lineStart == null || lineEnd == null)
            {
                Debug.LogError("[Skeleton Raid Spawner] Assign both Line Start and Line End transforms.", this);
                return false;
            }

            if (GameController.Data == null || GameController.Data.EnemiesDatabase == null)
            {
                Debug.LogError("[Skeleton Raid Spawner] Enemies Database is not available.", this);
                return false;
            }

            EnsureSpawnPoints();

            for (var i = 0; i < enemyCount; i++)
            {
                var enemy = GameController.Data.EnemiesDatabase.GetEnemyBehavior(enemyType);

                if (enemy == null)
                {
                    Debug.LogError($"[Skeleton Raid Spawner] Enemy of type {enemyType} is missing in the Enemies Database.", this);
                    continue;
                }

                var spawnPoint = spawnPoints[i];

                SimpleCallback handler = null;
                handler = () => OnEnemyDied(enemy);
                enemy.OnDeath += handler;

                deathHandlers.Add(enemy, handler);
                aliveEnemies.Add(enemy);

                enemy.Spawn(spawnPoint);
            }

            TotalSpawned = aliveEnemies.Count;
            IsActive = TotalSpawned > 0;

            return IsActive;
        }

        public void StopWave()
        {
            for (var i = aliveEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = aliveEnemies[i];

                if (enemy == null)
                    continue;

                if (deathHandlers.TryGetValue(enemy, out SimpleCallback handler))
                    enemy.OnDeath -= handler;

                if (!enemy.IsDead)
                    enemy.Unload();
            }

            aliveEnemies.Clear();
            deathHandlers.Clear();
            IsActive = false;
        }

        private void OnEnemyDied(BaseEnemyBehavior enemy)
        {
            if (enemy != null && deathHandlers.TryGetValue(enemy, out SimpleCallback handler))
                enemy.OnDeath -= handler;

            deathHandlers.Remove(enemy);
            aliveEnemies.Remove(enemy);

            EnemyDefeated?.Invoke();

            if (aliveEnemies.Count == 0)
            {
                IsActive = false;
                WaveCleared?.Invoke();
            }
        }

        private void EnsureSpawnPoints()
        {
            if (spawnPoints.Count == enemyCount)
            {
                UpdateSpawnPointPositions();
                return;
            }

            ClearSpawnPoints();

            for (var i = 0; i < enemyCount; i++)
            {
                var pointObject = new GameObject($"Raid Spawn Point {i}");
                pointObject.transform.SetParent(transform);
                spawnPoints.Add(pointObject.transform);
            }

            UpdateSpawnPointPositions();
        }

        private void UpdateSpawnPointPositions()
        {
            for (var i = 0; i < spawnPoints.Count; i++)
            {
                var t = spawnPoints.Count == 1 ? 0.5f : (float)i / (spawnPoints.Count - 1);
                var position = Vector3.Lerp(lineStart.position, lineEnd.position, t);

                if (NavMesh.SamplePosition(position, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
                    position = hit.position;

                spawnPoints[i].position = position;

                if (facingTarget != null)
                {
                    var lookDirection = facingTarget.position - position;
                    lookDirection.y = 0f;

                    if (lookDirection.sqrMagnitude > Mathf.Epsilon)
                        spawnPoints[i].rotation = Quaternion.LookRotation(lookDirection.normalized);
                }
                else
                {
                    spawnPoints[i].rotation = transform.rotation;
                }
            }
        }

        private void ClearSpawnPoints()
        {
            for (var i = 0; i < spawnPoints.Count; i++)
            {
                if (spawnPoints[i] != null)
                    Destroy(spawnPoints[i].gameObject);
            }

            spawnPoints.Clear();
        }

        private void OnDestroy()
        {
            StopWave();
        }

        private void OnDrawGizmosSelected()
        {
            if (lineStart == null || lineEnd == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(lineStart.position, lineEnd.position);
            Gizmos.DrawWireSphere(lineStart.position, 0.4f);
            Gizmos.DrawWireSphere(lineEnd.position, 0.4f);

            for (var i = 0; i < enemyCount; i++)
            {
                var t = enemyCount == 1 ? 0.5f : (float)i / (enemyCount - 1);
                Gizmos.DrawWireSphere(Vector3.Lerp(lineStart.position, lineEnd.position, t), 0.25f);
            }
        }

        #region Periodic Raids

        // TODO: Reuse this spawner to trigger recurring base attacks (independent of the mission).
        //
        // Intended design (left unimplemented on purpose):
        //   - Add serialized fields, e.g.:
        //         [SerializeField] bool autoRepeat;
        //         [SerializeField] Vector2 repeatDelayRange = new Vector2(60f, 120f);
        //   - On enable (when autoRepeat is on) start a timer; when it elapses and no wave IsActive,
        //     call SpawnWave(); after WaveCleared, restart the timer with a fresh random delay.
        //   - Optionally gate spawning on player/base proximity or on a controller that owns the
        //     "world is under attack" state, mirroring EnemySpawnPoint's playerDetectionRadius check.
        //
        // The public API above (SpawnWave / StopWave / IsActive / EnemyDefeated / WaveCleared) is
        // already sufficient to drive periodic raids without any further changes to this class.

        #endregion
    }
}
