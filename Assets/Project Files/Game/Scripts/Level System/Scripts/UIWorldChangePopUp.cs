using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [System.Serializable]
    public class UIWorldChangePopUp
    {
        private const string DESTINATIONS_POOL_NAME = "World Destination Button";

        [SerializeField] GameObject panelObject;
        [SerializeField] UIFadeAnimation fadeAnimation;
        [SerializeField] UIScaleAnimation panelBackScaleAnimation;

        [Space]
        [SerializeField] GameObject destinationButtonPrefab;
        [SerializeField] RectTransform destinationsContainer;

        [Space]
        [SerializeField] Button exitButton;
        [SerializeField] Button bigExitButton;

        private PoolGeneric<UIWorldDestinationButton> destinationsPool;

        private readonly List<WorldData> destinations = new List<WorldData>();
        private readonly List<UIWorldDestinationButton> spawnedButtons = new List<UIWorldDestinationButton>();

        private WorldDataCallback callback;

        public void Initialise()
        {
            exitButton.onClick.AddListener(Hide);
            bigExitButton.onClick.AddListener(Hide);
        }

        public void Unload()
        {
            ReleaseButtons();

            destinationsPool?.Destroy();
            destinationsPool = null;

            callback = null;
        }

        public void Show(WorldDataCallback callback)
        {
            this.callback = callback;

            SpawnDestinationButtons();

            panelObject.SetActive(true);
            fadeAnimation.Show();
            panelBackScaleAnimation.Show();

            UIGamepadButton.DisableAllTags();
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Popup);
        }

        public void Hide()
        {
            callback = null;

            fadeAnimation.Hide();
            panelBackScaleAnimation.Hide(onCompleted: () =>
            {
                panelObject.SetActive(false);

                ReleaseButtons();

                UIGamepadButton.DisableAllTags();
                UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);
            });
        }

        #region Destinations

        private void SpawnDestinationButtons()
        {
            ReleaseButtons();

            WorldController.GetTravelDestinations(destinations);

            if (destinations.Count == 0)
            {
                Debug.LogWarning("[World Change]: there are no other worlds to travel to.");

                return;
            }

            for (var i = 0; i < destinations.Count; i++)
            {
                var destinationButton = GetPool().GetPooledComponent();
                destinationButton.transform.SetAsLastSibling();
                destinationButton.Initialise(destinations[i], OnDestinationSelected);

                spawnedButtons.Add(destinationButton);
            }
        }

        private void ReleaseButtons()
        {
            if (destinationsPool == null)
            {
                spawnedButtons.Clear();

                return;
            }

            for (var i = 0; i < spawnedButtons.Count; i++)
            {
                var destinationButton = spawnedButtons[i];
                if (destinationButton == null)
                    continue;

                destinationButton.Clear();

                destinationsPool.ReturnToPool(destinationButton.gameObject);
            }

            spawnedButtons.Clear();
        }

        private PoolGeneric<UIWorldDestinationButton> GetPool()
        {
            destinationsPool ??= new PoolGeneric<UIWorldDestinationButton>(destinationButtonPrefab, DESTINATIONS_POOL_NAME, destinationsContainer);

            return destinationsPool;
        }

        private void OnDestinationSelected(WorldData worldData)
        {
            var selectionCallback = callback;
            callback = null;

            panelObject.SetActive(false);
            fadeAnimation.Hide(immediately: true);
            panelBackScaleAnimation.Hide(immediately: true);

            ReleaseButtons();

            selectionCallback?.Invoke(worldData);
        }

        #endregion
    }
}
