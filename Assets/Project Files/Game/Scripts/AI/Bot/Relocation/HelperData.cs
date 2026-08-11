using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Helper Data", menuName = "Data/Helper Data")]
    public class HelperData : ScriptableObject
    {
        [UniqueID, Order(-1)]
        [SerializeField] string globalId;
        public string GlobalId => globalId;

        [SerializeField] string displayName;
        public string DisplayName => displayName;

        [Space]
        [WorldPicker]
        [SerializeField] int homeWorldIndex;
        public int HomeWorldIndex => homeWorldIndex;

        [SerializeField] HelperBehavior guestPrefab;
        public HelperBehavior GuestPrefab => guestPrefab;

        public string HomeWorldId
        {
            get
            {
                var worldData = WorldController.GetWorldData(homeWorldIndex);

                return worldData != null ? worldData.ID : string.Empty;
            }
        }
    }
}
