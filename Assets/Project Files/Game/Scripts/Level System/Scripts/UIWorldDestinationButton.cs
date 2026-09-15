using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(Button))]
    public class UIWorldDestinationButton : MonoBehaviour
    {
        [SerializeField] TMP_Text nameText;

        private Button button;

        private WorldData worldData;
        public WorldData WorldData => worldData;

        private WorldDataCallback callback;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);
        }

        public void Initialise(WorldData worldData, WorldDataCallback callback)
        {
            this.worldData = worldData;
            this.callback = callback;

            nameText.text = worldData != null ? worldData.DisplayName : string.Empty;
        }

        public void Clear()
        {
            worldData = null;
            callback = null;
        }

        private void OnButtonClicked()
        {
            if (worldData == null)
                return;

            callback?.Invoke(worldData);
        }
    }
}
