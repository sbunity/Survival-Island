using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class WorldTravelManifest : MonoBehaviour
    {
        [SerializeField] HelperData[] travellingHelpers;

        private readonly List<HelperBehavior> boardingHelpers = new();
        private readonly List<IRaftPassenger> passengers = new();

        private bool isCollected;

        public IReadOnlyList<IRaftPassenger> CollectPassengers()
        {
            Collect();

            return passengers;
        }

        public void CommitTravel(string destinationWorldId)
        {
            if (string.IsNullOrEmpty(destinationWorldId))
                return;

            Collect();

            foreach (var helper in boardingHelpers)
            {
                HelperRoster.Move(helper.HelperData, destinationWorldId, true);

                helper.MarkAsRelocated();
            }

            boardingHelpers.Clear();
            passengers.Clear();

            isCollected = false;
        }

        private void Collect()
        {
            if (isCollected)
                return;

            isCollected = true;

            boardingHelpers.Clear();
            passengers.Clear();

            if (travellingHelpers.IsNullOrEmpty())
                return;

            var world = WorldController.WorldBehavior;
            if (world == null)
                return;

            var worldHelpers = world.GetComponentsInChildren<HelperBehavior>(true);

            foreach (var helperData in travellingHelpers)
            {
                if (helperData == null)
                    continue;

                var helper = FindHelper(worldHelpers, helperData);
                if (helper == null)
                    continue;

                boardingHelpers.Add(helper);
                passengers.Add(helper);
            }
        }

        private HelperBehavior FindHelper(HelperBehavior[] worldHelpers, HelperData helperData)
        {
            foreach (var helper in worldHelpers)
            {
                if (helper == null || helper.HelperData != helperData)
                    continue;

                return helper.IsOpened ? helper : null;
            }

            return null;
        }
    }
}
