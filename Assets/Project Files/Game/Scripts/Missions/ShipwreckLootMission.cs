using UnityEngine;

namespace Watermelon
{
    public sealed class ShipwreckLootMission : Mission
    {
        public override MissionUICase.Type MissionUIType => MissionUICase.Type.Task;

        [BoxGroup("Shipwreck Loot Mission Special", "Shipwreck Loot Mission Special")]
        [SerializeField] private ShipwreckLootBehavior shipwreckLoot;
        public ShipwreckLootBehavior ShipwreckLoot => shipwreckLoot;

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

            if (shipwreckLoot.IsLooted)
            {
                FinishMission();
            }
            else
            {
                shipwreckLoot.Looted += OnShipwreckLooted;
                StartMission();
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();

            if (shipwreckLoot != null)
                shipwreckLoot.Looted -= OnShipwreckLooted;
        }

        private void OnShipwreckLooted()
        {
            isDirty = true;
            FinishMission();
        }

        public override float GetProgress() 
            => shipwreckLoot.IsLooted ? 1.0f : 0.0f;

        public override Vector3 GetDefaultPreviewPosition() 
            => shipwreckLoot.transform.position;

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
