using UnityEngine;

namespace Watermelon
{
    public delegate void WorldDataCallback(WorldData worldData);

    [System.Serializable]
    public class WorldData
    {
        private const string UNLOCK_SAVE_FORMAT = "worldUnlock_{0}";

        [SerializeField] SceneObject scene;
        public SceneObject Scene => scene;

        [UniqueID]
        [SerializeField] string id;
        public string ID => id;

        [SerializeField] string displayName;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? scene.Name : displayName;

        [SerializeField] bool unlockedByDefault;
        public bool UnlockedByDefault => unlockedByDefault;

        private WorldUnlockSave unlockSave;

        public bool IsUnlocked => unlockedByDefault || (unlockSave != null && unlockSave.IsUnlocked);

        public void Initialise()
        {
            unlockSave = SaveController.GetSaveObject<WorldUnlockSave>(string.Format(UNLOCK_SAVE_FORMAT, id));
        }

        public void Unlock()
        {
            if (unlockSave == null)
            {
                Debug.LogError(string.Format("[Worlds]: world '{0}' is not initialised, unlock is ignored.", id));

                return;
            }

            if (unlockSave.IsUnlocked)
                return;

            unlockSave.IsUnlocked = true;

            SaveController.MarkAsSaveIsRequired();
        }

        public void Lock()
        {
            if (unlockSave == null || !unlockSave.IsUnlocked)
                return;

            unlockSave.IsUnlocked = false;

            SaveController.MarkAsSaveIsRequired();
        }
    }
}
