using UnityEngine;

namespace Watermelon
{
    public class PeriodicRaidScheduler : MonoBehaviour
    {
        [SerializeField] SkeletonRaidSpawner spawner;

        [Header("Raid Size")]
        [Tooltip("Random number of skeletons per raid (inclusive on both ends).")]
        [SerializeField] Vector2Int enemyCountRange = new Vector2Int(4, 8);

        [Header("Interval")]
        [Tooltip("Random delay between raids, in game-time seconds (SaveController GameTime).")]
        [SerializeField] Vector2 intervalRange = new Vector2(120f, 240f);

        [Header("Save")]
        [UniqueID]
        [SerializeField] string uniqueSaveID;

        private const float SpawnRetryDelay = 5f;

        private PeriodicRaidSave save;
        private bool isInitialised;
        private bool subscribed;

        private static float GameTime => SaveController.GetSaveObject<TimeSave>().GameTime;

        private void OnEnable()
        {
            Tween.NextFrame(Initialise);
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Initialise()
        {
            if (this == null || !isActiveAndEnabled)
                return;

            if (!isInitialised)
            {
                if (spawner == null)
                {
                    Debug.LogError("[Periodic Raid Scheduler] Spawner reference is missing.", this);
                    enabled = false;
                    return;
                }

                if (string.IsNullOrEmpty(uniqueSaveID))
                {
                    Debug.LogError("[Periodic Raid Scheduler] Unique save ID is not assigned.", this);
                    enabled = false;
                    return;
                }

                if (WorldController.CurrentWorld == null)
                    return;

                var worldSave = SaveController.GetFile(WorldController.CurrentWorld.ID);
                save = worldSave.GetSaveObject<PeriodicRaidSave>(uniqueSaveID);

                if (save.NextRaidAtGameTime <= 0f)
                    ScheduleNext();

                isInitialised = true;
            }

            Subscribe();
        }

        private void Update()
        {
            if (!isInitialised || spawner.IsActive)
                return;

            if (GameTime < save.NextRaidAtGameTime)
                return;

            var count = Random.Range(enemyCountRange.x, enemyCountRange.y + 1);

            if (!spawner.SpawnWave(count))
            {
                save.NextRaidAtGameTime = GameTime + SpawnRetryDelay;
                return;
            }

        }

        private void ScheduleNext()
        {
            var delay = Mathf.Max(0f, Random.Range(intervalRange.x, intervalRange.y));
            save.NextRaidAtGameTime = GameTime + delay;
        }

        private void Subscribe()
        {
            if (subscribed || spawner == null)
                return;

            spawner.WaveCleared += OnWaveCleared;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || spawner == null)
                return;

            spawner.WaveCleared -= OnWaveCleared;
            subscribed = false;
        }

        private void OnWaveCleared()
        {
            ScheduleNext();
        }

        private void OnValidate()
        {
            enemyCountRange.x = Mathf.Max(1, enemyCountRange.x);
            enemyCountRange.y = Mathf.Max(enemyCountRange.x, enemyCountRange.y);
            intervalRange.x = Mathf.Max(0f, intervalRange.x);
            intervalRange.y = Mathf.Max(intervalRange.x, intervalRange.y);
        }
    }
}
