using UnityEngine;

namespace Watermelon
{
    public sealed class DefeatRaidMission : Mission
    {
        public override MissionUICase.Type MissionUIType => MissionUICase.Type.Collect;

        [BoxGroup("Defeat Raid Mission Special", "Defeat Raid Mission Special")]
        [SerializeField] SkeletonRaidSpawner spawner;
        public SkeletonRaidSpawner Spawner => spawner;

        private Save save;

        public override void Initialise()
        {
            base.Initialise();

            var worldData = WorldController.CurrentWorld;
            var worldSave = SaveController.GetFile(worldData.ID);

            save = worldSave.GetSaveObject<Save>(GetSaveString());
            save.LinkMission(this);

            missionStage = save.MissionStage;
        }

        public override void Activate()
        {
            base.Activate();

            isDirty = true;

            if (missionStage == Stage.Finished)
            {
                FinishMission();
                return;
            }

            if (missionStage == Stage.Collected)
                return;

            if (spawner == null)
            {
                Debug.LogError("[Defeat Raid Mission] Spawner reference is missing.", this);
                return;
            }

            spawner.EnemyDefeated -= OnEnemyDefeated;
            spawner.EnemyDefeated += OnEnemyDefeated;
            spawner.WaveCleared -= OnWaveCleared;
            spawner.WaveCleared += OnWaveCleared;

            if (!spawner.SpawnWave())
            {
                spawner.EnemyDefeated -= OnEnemyDefeated;
                spawner.WaveCleared -= OnWaveCleared;
                return;
            }

            StartMission();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            if (spawner != null)
            {
                spawner.EnemyDefeated -= OnEnemyDefeated;
                spawner.WaveCleared -= OnWaveCleared;
                spawner.StopWave();
            }
        }

        private void OnEnemyDefeated()
        {
            isDirty = true;
        }

        private void OnWaveCleared()
        {
            isDirty = true;
            FinishMission();
        }

        public override string GetFormattedProgress()
        {
            if (spawner == null || spawner.TotalSpawned == 0)
                return string.Empty;

            var killed = spawner.TotalSpawned - spawner.AliveCount;
            return string.Format("{0}/{1}", killed, spawner.TotalSpawned);
        }

        public override float GetProgress()
        {
            if (missionStage == Stage.Finished || missionStage == Stage.Collected)
                return 1.0f;

            if (spawner == null || spawner.TotalSpawned == 0)
                return 0.0f;

            var killed = spawner.TotalSpawned - spawner.AliveCount;
            return Mathf.InverseLerp(0, spawner.TotalSpawned, killed);
        }

        public override Vector3 GetDefaultPreviewPosition() 
            => spawner != null ? spawner.Center : transform.position;

        [System.Serializable]
        public class Save : MissionSave
        {

        }
    }
}
