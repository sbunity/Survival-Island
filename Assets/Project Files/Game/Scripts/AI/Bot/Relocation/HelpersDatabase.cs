using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Helpers Database", menuName = "Data/Helpers Database")]
    public class HelpersDatabase : ScriptableObject
    {
        [SerializeField] HelperData[] helpers;
        public HelperData[] Helpers => helpers;

        public HelperData GetHelperData(string globalId)
        {
            if (string.IsNullOrEmpty(globalId) || helpers.IsNullOrEmpty())
                return null;

            for (var i = 0; i < helpers.Length; i++)
            {
                if (helpers[i] != null && helpers[i].GlobalId == globalId)
                    return helpers[i];
            }

            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (helpers.IsNullOrEmpty())
                return;

            for (var i = 0; i < helpers.Length; i++)
            {
                var helper = helpers[i];
                if (helper == null)
                    continue;

                for (var j = i + 1; j < helpers.Length; j++)
                {
                    var other = helpers[j];
                    if (other == null)
                        continue;

                    if (helper.GlobalId == other.GlobalId)
                        Debug.LogError($"[Helpers Database]: '{helper.name}' and '{other.name}' share the same global ID!", this);

                    if (helper.GuestPrefab != null && helper.GuestPrefab == other.GuestPrefab)
                        Debug.LogError($"[Helpers Database]: '{helper.name}' and '{other.name}' share the same guest prefab! Every helper needs its own prefab.", this);
                }
            }
        }
#endif
    }
}
