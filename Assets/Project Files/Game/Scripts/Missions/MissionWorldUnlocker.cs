using UnityEngine;

namespace Watermelon
{
    public class MissionWorldUnlocker : MonoBehaviour
    {
        [SerializeField] Mission mission;

        [WorldPicker]
        [SerializeField] int[] worldsToUnlock;

        private void Awake()
        {
            if (mission == null)
                mission = GetComponent<Mission>();
        }

        private void OnEnable()
        {
            if (mission != null)
                mission.OnStageChanged += OnMissionStageChanged;

            WorldController.OnWorldLoaded += OnWorldLoaded;
        }

        private void OnDisable()
        {
            if (mission != null)
                mission.OnStageChanged -= OnMissionStageChanged;

            WorldController.OnWorldLoaded -= OnWorldLoaded;
        }

        private void OnWorldLoaded()
        {
            UnlockWorlds();
        }

        private void OnMissionStageChanged(Mission.Stage previousStage, Mission.Stage currentStage)
        {
            UnlockWorlds();
        }

        private void UnlockWorlds()
        {
            if (mission == null)
            {
                Debug.LogError("[World Unlocker]: mission reference is missing!", gameObject);

                return;
            }

            if (mission.MissionStage < Mission.Stage.Finished)
                return;

            if (worldsToUnlock.IsNullOrEmpty())
                return;

            for (var i = 0; i < worldsToUnlock.Length; i++)
            {
                var worldIndex = worldsToUnlock[i];

                if (!WorldController.IsWorldExists(worldIndex))
                {
                    Debug.LogError("[World Unlocker]: incorrect world index!", gameObject);

                    continue;
                }

                WorldController.UnlockWorld(WorldController.GetWorldData(worldIndex));
            }
        }
    }
}
