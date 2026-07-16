using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon
{
    public sealed class GuardedHelperMission : Mission
    {
        public override MissionUICase.Type MissionUIType => MissionUICase.Type.Task;

        [BoxGroup("Guarded Rescue Mission Special", "Guarded Rescue Mission Special")]
        [FormerlySerializedAs("helperBehavior")]
        [SerializeField] MonoBehaviour rescueTargetBehaviour;

        [BoxGroup("Guarded Rescue Mission Special")]
        [SerializeField] GuardedSkeletonEncounter encounter;
        public GuardedSkeletonEncounter Encounter => encounter;

        private IGuardedRescueTarget rescueTarget;

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

            rescueTarget = rescueTargetBehaviour as IGuardedRescueTarget;

            if (rescueTarget == null || encounter == null)
            {
                Debug.LogError("[Guarded Rescue Mission] Rescue target (must implement IGuardedRescueTarget) or encounter reference is missing.", this);
                return;
            }

            if (!rescueTarget.WaitForExternalRelease)
            {
                Debug.LogError("[Guarded Rescue Mission] Enable 'Wait For External Release' on the linked rescue target.", rescueTargetBehaviour);
                return;
            }

            rescueTarget.RescueAreaUnlocked -= OnRescueAreaUnlocked;
            rescueTarget.RescueAreaUnlocked += OnRescueAreaUnlocked;
            encounter.EnemyDied -= OnEnemyDied;
            encounter.EnemyDied += OnEnemyDied;

            var startLocked = !rescueTarget.IsRescued && !rescueTarget.IsRescueAreaUnlocked;

            if (!encounter.Begin(startLocked))
            {
                rescueTarget.RescueAreaUnlocked -= OnRescueAreaUnlocked;
                encounter.EnemyDied -= OnEnemyDied;
                return;
            }

            StartMission();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            if (rescueTarget != null)
                rescueTarget.RescueAreaUnlocked -= OnRescueAreaUnlocked;

            if (encounter != null)
            {
                encounter.EnemyDied -= OnEnemyDied;
                encounter.Stop();
            }
        }

        private void OnRescueAreaUnlocked()
        {
            rescueTarget.RescueAreaUnlocked -= OnRescueAreaUnlocked;

            isDirty = true;
            encounter.UnlockCombat();
        }

        private void OnEnemyDied()
        {
            if (!rescueTarget.IsRescued && !rescueTarget.TryRelease())
            {
                Debug.LogError("[Guarded Rescue Mission] Rescue target cannot be released before its area is unlocked.", rescueTargetBehaviour);
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

            return rescueTargetBehaviour != null ? rescueTargetBehaviour.transform.position : transform.position;
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
