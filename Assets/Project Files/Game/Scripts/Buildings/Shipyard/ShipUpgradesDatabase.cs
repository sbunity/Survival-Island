using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Ship Upgrades Database", menuName = "Data/Ship Upgrades Database")]
    public class ShipUpgradesDatabase : ScriptableObject
    {
        [SerializeField] ShipUpgradeStage[] stages;
        public ShipUpgradeStage[] Stages => stages;

        public ShipUpgradeStage GetStage(string id)
        {
            if (string.IsNullOrEmpty(id) || stages.IsNullOrEmpty())
                return null;

            for (var i = 0; i < stages.Length; i++)
            {
                if (stages[i] != null && stages[i].ID == id)
                    return stages[i];
            }

            return null;
        }

        public int GetStageIndex(string id)
        {
            if (string.IsNullOrEmpty(id) || stages.IsNullOrEmpty())
                return -1;

            for (var i = 0; i < stages.Length; i++)
            {
                if (stages[i] != null && stages[i].ID == id)
                    return i;
            }

            return -1;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (stages.IsNullOrEmpty())
                return;

            var seenIDs = new System.Collections.Generic.HashSet<string>();
            var isDirty = false;

            for (var i = 0; i < stages.Length; i++)
            {
                var stage = stages[i];
                if (stage == null)
                    continue;

                if (!string.IsNullOrEmpty(stage.ID) && seenIDs.Add(stage.ID))
                    continue;

                stage.EditorSetID(UniqueIDUtils.GetUniqueID());
                seenIDs.Add(stage.ID);
                isDirty = true;
            }

            if (isDirty)
                UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
