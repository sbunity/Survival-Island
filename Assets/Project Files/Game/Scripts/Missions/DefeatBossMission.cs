using UnityEngine;

namespace Watermelon
{
    public sealed class DefeatBossMission : Mission
    {
        public override MissionUICase.Type MissionUIType => MissionUICase.Type.Collect;

        [BoxGroup("Defeat Boss Mission Special", "Defeat Boss Mission Special")]
        [SerializeField] BossSkeletonBehavior boss;
        public BossSkeletonBehavior Boss => boss;

        private Save save;
        private bool isSubscribed;

        public override void Initialise()
        {
            base.Initialise();

            WorldData worldData = WorldController.CurrentWorld;
            SaveFile worldSave = SaveController.GetFile(worldData.ID);

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

            if (boss == null)
            {
                Debug.LogError("[Defeat Boss Mission] Boss reference is missing.", this);
                return;
            }

            if (boss.IsDefeated)
            {
                FinishMission();
                return;
            }

            Subscribe();

            StartMission();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            Unsubscribe();
        }

        public override void Unload()
        {
            base.Unload();

            Unsubscribe();
        }

        private void Subscribe()
        {
            if (isSubscribed || boss == null)
                return;

            boss.Defeated += OnBossDefeated;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed || boss == null)
                return;

            boss.Defeated -= OnBossDefeated;
            isSubscribed = false;
        }

        private void OnBossDefeated()
        {
            isDirty = true;

            Unsubscribe();

            FinishMission();
        }

        public override float GetProgress()
        {
            if (missionStage == Stage.Finished || missionStage == Stage.Collected)
                return 1.0f;

            return boss != null && boss.IsDefeated ? 1.0f : 0.0f;
        }

        public override string GetFormattedProgress()
        {
            var defeated = missionStage == Stage.Finished || missionStage == Stage.Collected
                || (boss != null && boss.IsDefeated);

            return string.Format("{0}/1", defeated ? 1 : 0);
        }

        public override Vector3 GetDefaultPreviewPosition()
            => boss != null ? boss.transform.position : transform.position;

        [System.Serializable]
        public class Save : MissionSave
        {

        }
    }
}
