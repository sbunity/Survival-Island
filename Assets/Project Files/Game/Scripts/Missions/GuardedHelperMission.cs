using UnityEngine;

namespace Watermelon
{
    public sealed class GuardedHelperMission : Mission
    {
        public override MissionUICase.Type MissionUIType => MissionUICase.Type.Task;

        [BoxGroup("Guarded Helper Mission Special", "Guarded Helper Mission Special")]
        [SerializeField] HelperBehavior helperBehavior;
        public HelperBehavior HelperBehavior => helperBehavior;

        [BoxGroup("Guarded Helper Mission Special")]
        [SerializeField] GuardedSkeletonEncounter encounter;
        public GuardedSkeletonEncounter Encounter => encounter;

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

            if (helperBehavior == null || encounter == null)
            {
                Debug.LogError("[Guarded Helper Mission] Helper or encounter reference is missing.", this);
                return;
            }

            if (!helperBehavior.WaitForExternalRelease)
            {
                Debug.LogError("[Guarded Helper Mission] Enable 'Wait For External Release' on the linked helper.", helperBehavior);
                return;
            }

            helperBehavior.OpeningAreaUnlocked += OnOpeningAreaUnlocked;
            encounter.EnemyDied += OnEnemyDied;

            var startLocked = !helperBehavior.IsOpened && !helperBehavior.IsOpeningAreaUnlocked;

            if (!encounter.Begin(startLocked))
            {
                helperBehavior.OpeningAreaUnlocked -= OnOpeningAreaUnlocked;
                encounter.EnemyDied -= OnEnemyDied;
                return;
            }

            StartMission();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            if (helperBehavior != null)
                helperBehavior.OpeningAreaUnlocked -= OnOpeningAreaUnlocked;

            if (encounter != null)
            {
                encounter.EnemyDied -= OnEnemyDied;
                encounter.Stop();
            }
        }

        private void OnOpeningAreaUnlocked()
        {
            helperBehavior.OpeningAreaUnlocked -= OnOpeningAreaUnlocked;

            isDirty = true;
            encounter.UnlockCombat();
        }

        private void OnEnemyDied()
        {
            if (!helperBehavior.IsOpened && !helperBehavior.TryRelease())
            {
                Debug.LogError("[Guarded Helper Mission] Helper cannot be released before its opening area is unlocked.", helperBehavior);
                return;
            }

            isDirty = true;
            FinishMission();
        }

        public override string GetFormattedProgress() 
            => "";

        public override float GetProgress() 
            => missionStage == Stage.Finished || missionStage == Stage.Collected ? 1.0f : 0.0f;

        public override Vector3 GetDefaultPreviewPosition()
        {
            if (encounter != null)
                return encounter.Position;

            return helperBehavior != null ? helperBehavior.transform.position : transform.position;
        }

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
