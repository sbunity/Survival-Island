using UnityEngine;

namespace Watermelon
{
    public sealed class GuardedHelperMission : Mission
    {
        public override MissionUICase.Type MissionUIType => MissionUICase.Type.Task;

        [BoxGroup("Guarded Rescue Mission Special", "Guarded Rescue Mission Special")]
        [SerializeField] GuardedSkeletonEncounter encounter;
        public GuardedSkeletonEncounter Encounter => encounter;

        private Save save;
        private bool isSubscribed;

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

            if (encounter == null)
            {
                Debug.LogError("[Guarded Rescue Mission] Encounter reference is missing.", this);
                return;
            }

            if (encounter.IsCleared)
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
            if (isSubscribed || encounter == null)
                return;

            encounter.Cleared += OnEncounterCleared;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed || encounter == null)
                return;

            encounter.Cleared -= OnEncounterCleared;
            isSubscribed = false;
        }

        private void OnEncounterCleared()
        {
            isDirty = true;

            Unsubscribe();

            FinishMission();
        }

        public override string GetFormattedProgress()
            => "";

        public override float GetProgress()
        {
            if (missionStage == Stage.Finished || missionStage == Stage.Collected)
                return 1.0f;

            return encounter != null && encounter.IsCleared ? 1.0f : 0.0f;
        }

        public override Vector3 GetDefaultPreviewPosition()
            => encounter != null ? encounter.Position : transform.position;

        #region Development

        [Button("Auto Adjust Pointer", "ShowCustomPointerFieldEditor", ButtonVisibility.ShowIf)]
        public void AutoAdjustPointer()
        {
            if (CustomPointerLocation != null)
            {
                CustomPointerLocation.position = GetDefaultPreviewPosition();
                RuntimeEditorUtils.SetDirty(CustomPointerLocation);
            }
        }

        #endregion

        [System.Serializable]
        public class Save : MissionSave
        {

        }
    }
}
