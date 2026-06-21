using UnityEngine;
using System.Collections.Generic;

namespace Watermelon
{
    using GlobalUpgrades;

    public class GlobalUpgradesController : MonoBehaviour
    {
        private static GlobalUpgradesController instance;

        private const string SAVE_IDENTIFIER = "upgrades:{0}";

        [SerializeField] GlobalUpgradesDatabase upgradesDatabase;

        private List<AbstactGlobalUpgrade> activeUpgrades;
        public static List<AbstactGlobalUpgrade> ActiveUpgrades => instance.activeUpgrades;

        private Dictionary<GlobalUpgradeType, AbstactGlobalUpgrade> activeUpgradesLink;

        private List<IUpgrade> globalSimpleUpgrades = new List<IUpgrade>();

        private UIUpgrades uiUpgrades;

        public void Initialise()
        {
            instance = this;

            WorldData worldData = WorldController.CurrentWorld;
            SaveFile worldSave = SaveController.GetFile(worldData.ID);

            activeUpgrades = new List<AbstactGlobalUpgrade>(upgradesDatabase.Upgrades);
            
            activeUpgradesLink = new Dictionary<GlobalUpgradeType, AbstactGlobalUpgrade>();
            for (int i = 0; i < activeUpgrades.Count; i++)
            {
                var upgrade = activeUpgrades[i];

                var hash = string.Format(SAVE_IDENTIFIER, upgrade.GlobalUpgradeType.ToString());

                UpgradeSavableObject save = worldSave.GetSaveObject<UpgradeSavableObject>(hash);

                upgrade.SetSave(save);

                if (!activeUpgradesLink.ContainsKey(upgrade.GlobalUpgradeType))
                {
                    upgrade.Initialise();

                    activeUpgradesLink.Add(upgrade.GlobalUpgradeType, activeUpgrades[i]);
                }
            }

            uiUpgrades = UIController.GetPage<UIUpgrades>();
        }

        private void Oestroy()
        {
            instance = null;            
        }

        [System.Obsolete]
        public static AbstactGlobalUpgrade GetUpgradeByType(GlobalUpgradeType perkType)
        {
            if (instance.activeUpgradesLink.ContainsKey(perkType))
                return instance.activeUpgradesLink[perkType];

            Debug.LogError($"[Perks]: Upgrade with type {perkType} isn't registered!");

            return null;
        }

        public static T GetUpgrade<T>(GlobalUpgradeType type) where T : AbstactGlobalUpgrade
        {
            if (instance.activeUpgradesLink.ContainsKey(type))
                return instance.activeUpgradesLink[type] as T;

            Debug.LogError($"[Perks]: Upgrade with type {type} isn't registered!");

            return null;
        }

        public static void RegisterSimpleUpgrade(IUpgrade upgrade)
        {
            instance.globalSimpleUpgrades.Add(upgrade);
        }

        public static void OpenMainUpgradesPage()
        {
            instance.uiUpgrades.ResetUpgrades();
            instance.uiUpgrades.RegisterUpgrades(instance.activeUpgrades.ConvertAll(upgrade => (IUpgrade)upgrade));
            instance.uiUpgrades.RegisterUpgrades(instance.globalSimpleUpgrades);

            UIController.ShowPage<UIUpgrades>();
        }

        public static void OpenUpgradesPage(List<IUpgrade> upgradesToOpen)
        {
            instance.uiUpgrades.ResetUpgrades();
            instance.uiUpgrades.RegisterUpgrades(upgradesToOpen);

            UIController.ShowPage<UIUpgrades>();
        }
    }
}