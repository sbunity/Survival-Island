using UnityEngine;

namespace Watermelon
{
    public sealed class OpenChestMission : Mission
    {
        public override MissionUICase.Type MissionUIType => MissionUICase.Type.Task;

        [BoxGroup("Open Chest Mission Special", "Open Chest Mission Special")]
        [SerializeField] private ChestBehavior targetChest;
        public ChestBehavior TargetChest => targetChest;

        private Save save;

        public override void Initialise()
        {
            base.Initialise();

            var worldData = WorldController.CurrentWorld;
            SaveFile worldSave = SaveController.GetFile(worldData.ID);

            save = worldSave.GetSaveObject<Save>(GetSaveString());
            save.LinkMission(this);

            missionStage = save.MissionStage;
        }

        public override void Activate()
        {
            base.Activate();

            isDirty = true;

            if (targetChest.IsOpened)
            {
                FinishMission();
            }
            else
            {
                targetChest.ChestOpened += OnChestOpened;
                StartMission();
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();

            if (targetChest != null)
                targetChest.ChestOpened -= OnChestOpened;
        }

        private void OnChestOpened(ChestBehavior chestBehavior)
        {
            isDirty = true;
            FinishMission();
        }

        public override float GetProgress()
            => targetChest.IsOpened ? 1.0f : 0.0f;

        public override Vector3 GetDefaultPreviewPosition()
            => targetChest.transform.position;

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
