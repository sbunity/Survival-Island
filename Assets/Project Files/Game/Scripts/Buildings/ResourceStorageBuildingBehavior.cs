using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class ResourceStorageBuildingBehavior : BuildingBehavior, IGroundOpenable
    {
        [SerializeField] List<CurrencyType> storedResources;
        public List<CurrencyType> StoredResources => storedResources;

        [SerializeField, HideIf("EditorHaveCapacityUpgrade")] int capacity;

        [Space]
        [SerializeField] SimpleResourceStorageBehavior storage;
        public SimpleResourceStorageBehavior Storage => storage;

        [Space]
        [SerializeField] bool isHelperTaskActive = true;
        public bool IsHelperTaskActive => isHelperTaskActive;

        [Space]
        [SerializeField] GameObject emptyStorageIndicator;
        [SerializeField] TMP_Text emptyStorageIndicatorText;

        public bool IsFull => storage.IsFull();

        private StoreResourcesTask storeResourcesTask;

        protected SimpleIntUpgrade StorageCapacityUpgrade { get; private set; }

        protected override void RegisterUpgrades()
        {
            for (int i = 0; i < buildingUpgrades.Count; i++)
            {
                var upgrade = buildingUpgrades[i];

                if (upgrade.UpgradeType == BuildingUpgradeType.StorageCapacity)
                {
                    StorageCapacityUpgrade = (SimpleIntUpgrade)upgrade.Upgrade;

                    string upgradeSaveName = $"{ID}_{BuildingUpgradeType.StorageCapacity}";
                    StorageCapacityUpgrade.Init(upgradeSaveName);

                    capacity = StorageCapacityUpgrade.CurrentStage.Value;

                    StorageCapacityUpgrade.OnUpgraded += OnCapacityUpgraded;
                }
            }
        }

        protected override void Init()
        {
            base.Init();

            string storageSaveName = $"{ID}_Storage";
            storage.Init(storageSaveName, storedResources, capacity);

            storeResourcesTask = new StoreResourcesTask(this);
            storeResourcesTask.Activate();
            storeResourcesTask.Register(LinkedWorldBehavior.TaskHandler);

            storage.OnResourcesChanged -= OnStorageResourcesChanged;
            storage.OnResourcesChanged += OnStorageResourcesChanged;
            OnStorageResourcesChanged();

            emptyStorageIndicatorText.text = "0/" + capacity;
        }

        private void OnStorageResourcesChanged()
        {
            emptyStorageIndicator.SetActive(storage.IsEmpty());
        }

        private void OnCapacityUpgraded()
        {
            capacity = StorageCapacityUpgrade.CurrentStage.Value;
            emptyStorageIndicatorText.text = "0/" + capacity;

            Storage.SetCapacity(capacity);
        }

        public override void OnWorldLoaded()
        {
            base.OnWorldLoaded();
        }

        public override void OnWorldUnloaded()
        {
            base.OnWorldUnloaded();
        }

        public void OnGroundOpen(bool immediately = false)
        {
            gameObject.SetActive(true);
        }

        public void OnGroundHidden(bool immediately = false)
        {
            gameObject.SetActive(false);
        }

        [Button]
        public void MakeFull()
        {
            while(!Storage.IsFull())
            {
                Storage.AddResources(Resource.One(Storage.RequiredResources.GetRandomItem()));
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (storage != null)
                storage.OnResourcesChanged -= OnStorageResourcesChanged;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (storage != null)
            {
                storage.OnResourcesChanged -= OnStorageResourcesChanged;
                storage.OnResourcesChanged += OnStorageResourcesChanged;
            }
        }

        protected override void OnOperationalStateChanged(bool isOperational)
        {
            if (storeResourcesTask == null)
                return;

            if (isOperational)
                storeResourcesTask.Activate();
            else
                storeResourcesTask.Disable();
        }
    }
}
