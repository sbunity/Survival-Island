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
        [Tooltip("Random delay between raids, in seconds of active gameplay.")]
        [SerializeField] Vector2 intervalRange = new Vector2(120f, 240f);

        [Header("Save")]
        [UniqueID]
        [SerializeField] string uniqueSaveID;

        [Header("Debug")]
        [Tooltip("Interval, in seconds, chosen for the current wait until the next raid.")]
        [SerializeField, ReadOnly] float selectedInterval;
        [Tooltip("Seconds already elapsed within the current interval.")]
        [SerializeField, ReadOnly] float secondsElapsed;

        private const float SpawnRetryDelay = 5f;

        private PeriodicRaidSave save;
        private bool isInitialised;

        private void OnEnable()
        {
            Tween.NextFrame(Initialise);
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

                isInitialised = true;
            }

            if (save.SelectedInterval <= 0f)
                ScheduleNext();

            SyncDebug();
        }

        private void Update()
        {
            if (!isInitialised)
                return;

            save.SecondsElapsed += Time.deltaTime;
            SyncDebug();

            if (save.SecondsElapsed < save.SelectedInterval)
                return;

            var count = Random.Range(enemyCountRange.x, enemyCountRange.y + 1);

            if (!spawner.SpawnWave(count))
            {
                save.SelectedInterval = SpawnRetryDelay;
                save.SecondsElapsed = 0f;
                SyncDebug();
                return;
            }

            ScheduleNext();
        }

        private void ScheduleNext()
        {
            save.SelectedInterval = Mathf.Max(0f, Random.Range(intervalRange.x, intervalRange.y));
            save.SecondsElapsed = 0f;
            SyncDebug();
        }

        private void SyncDebug()
        {
            selectedInterval = save.SelectedInterval;
            secondsElapsed = save.SecondsElapsed;
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
