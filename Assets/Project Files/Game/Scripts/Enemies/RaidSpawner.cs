using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public abstract class RaidSpawner : MonoBehaviour
    {
        [SerializeField] protected EnemyType enemyType = EnemyType.Skeleton;

        [BoxFoldout("Wave", label: "Wave")]
        [SerializeField, Min(1)] int enemyCount = 10;
        protected int EnemyCount => enemyCount;

        public bool IsActive { get; private set; }
        public int AliveCount => aliveEnemies.Count;
        public int TotalSpawned { get; private set; }

        public abstract Vector3 Center { get; }

        public virtual int MaxWaveSize => int.MaxValue;

        public event SimpleCallback EnemyDefeated;
        public event SimpleCallback WaveCleared;

        private readonly List<BaseEnemyBehavior> aliveEnemies = new();
        private readonly Dictionary<BaseEnemyBehavior, SimpleCallback> deathHandlers = new();

        private bool holdsRaidState;

        public bool SpawnWave() => SpawnWave(enemyCount);

        public bool SpawnWave(int count)
        {
            StopWave();

            var waveSize = Mathf.Clamp(count, 1, Mathf.Max(1, MaxWaveSize));

            if (!PrepareWave(waveSize))
                return false;

            if (GameController.Data == null || GameController.Data.EnemiesDatabase == null)
            {
                Debug.LogError($"[{GetType().Name}] Enemies Database is not available.", this);
                return false;
            }

            for (var i = 0; i < waveSize; i++)
            {
                var enemy = GameController.Data.EnemiesDatabase.GetEnemyBehavior(enemyType);

                if (enemy == null)
                {
                    Debug.LogError($"[{GetType().Name}] Enemy of type {enemyType} is missing in the Enemies Database.", this);
                    continue;
                }

                Track(enemy);

                if (!SpawnEnemy(enemy, i))
                {
                    Untrack(enemy);
                    enemy.Unload();
                }
            }

            TotalSpawned = aliveEnemies.Count;
            IsActive = TotalSpawned > 0;

            SetRaidStateHeld(IsActive);

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

            SetRaidStateHeld(false);
        }

        protected abstract bool PrepareWave(int count);

        protected abstract bool SpawnEnemy(BaseEnemyBehavior enemy, int index);

        private void Track(BaseEnemyBehavior enemy)
        {
            SimpleCallback handler = null;
            handler = () => OnEnemyDied(enemy);
            enemy.OnDeath += handler;

            deathHandlers.Add(enemy, handler);
            aliveEnemies.Add(enemy);
        }

        private void Untrack(BaseEnemyBehavior enemy)
        {
            if (enemy != null && deathHandlers.TryGetValue(enemy, out SimpleCallback handler))
                enemy.OnDeath -= handler;

            deathHandlers.Remove(enemy);
            aliveEnemies.Remove(enemy);
        }

        private void SetRaidStateHeld(bool value)
        {
            if (holdsRaidState == value)
                return;

            holdsRaidState = value;

            if (value)
                RaidState.Acquire();
            else
                RaidState.Release();
        }

        private void OnEnemyDied(BaseEnemyBehavior enemy)
        {
            Untrack(enemy);

            EnemyDefeated?.Invoke();

            if (aliveEnemies.Count == 0)
            {
                IsActive = false;

                SetRaidStateHeld(false);

                WaveCleared?.Invoke();
            }
        }

        protected virtual void OnDestroy()
        {
            StopWave();
        }
    }
}
