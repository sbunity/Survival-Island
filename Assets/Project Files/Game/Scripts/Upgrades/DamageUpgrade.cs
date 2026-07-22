using UnityEngine;
using Watermelon.GlobalUpgrades;
using System;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Damage Upgrade", menuName = "Data/Upgrades/Damage Upgrade")]
    public class DamageUpgrade : GlobalUpgrade<DamageUpgrade.DamageStage>
    {
        public override void Initialise()
        {

        }

        public override string GetUpgradeDescription(int stageId)
        {
            try
            {
                var prevValue = GetStage(stageId).Damage;
                var value = GetStage(stageId + 1).Damage;

                return string.Format(DescriptionFormat, prevValue, value);
            }
            catch
            {
                return "";
            }
        }

        [Serializable]
        public class DamageStage : GlobalUpgradeStage
        {
            [SerializeField] int damage = 1;
            public int Damage => damage;
        }
    }
}
