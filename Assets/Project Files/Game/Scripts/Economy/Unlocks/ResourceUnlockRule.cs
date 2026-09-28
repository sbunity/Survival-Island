using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public struct ResourceUnlockRule
    {
        [SerializeField] CurrencyType currency;
        public CurrencyType Currency => currency;

        [SerializeField] UnlockMode mode;
        public UnlockMode Mode => mode;

        [SerializeField, MissionPicker] string missionId;
        public string MissionId => missionId;

        public bool IsUnlocked
        {
            get
            {
                return mode switch
                {
                    UnlockMode.FromStart => true,
                    UnlockMode.AfterMission => MissionProgress.IsCompleted(missionId),
                    _ => false,
                };

            }
        }

        public ResourceUnlockRule(CurrencyType currency, UnlockMode mode, string missionId)
        {
            this.currency = currency;
            this.mode = mode;
            this.missionId = missionId;
        }

        public enum UnlockMode
        {
            FromStart = 0,
            AfterMission = 1,
            Never = 2
        }
    }
}
