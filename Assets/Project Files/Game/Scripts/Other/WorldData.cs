using UnityEngine;

namespace Watermelon
{
    public delegate void WorldDataCallback(WorldData worldData);

    [System.Serializable]
    public class WorldData
    {
        [SerializeField] SceneObject scene;
        public SceneObject Scene => scene;

        [UniqueID]
        [SerializeField] string id;
        public string ID => id;

        [SerializeField] string displayName;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? scene.Name : displayName;
    }
}
