using UnityEngine;
using Watermelon.GlobalUpgrades;
using System;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Max Health Upgrade", menuName = "Data/Upgrades/Max Health Upgrade")]
    public class MaxHealthUpgrade : GlobalUpgrade<MaxHealthUpgrade.MaxHealthStage>
    {
        public override void Initialise()
        {

        }

        public override string GetUpgradeDescription(int stageId)
        {
            try
            {
                var prevValue = GetStage(stageId).MaxHealth;
                var value = GetStage(stageId + 1).MaxHealth;

                return string.Format(DescriptionFormat, prevValue, value);
            }
            catch
            {
                return "";
            }
        }

        [Serializable]
        public class MaxHealthStage : GlobalUpgradeStage
        {
            [SerializeField] int maxHealth = 100;
            public int MaxHealth => maxHealth;
        }
    }
}
