namespace Watermelon
{
    public sealed class RescueAreaGate
    {
        private GroundTileComplexBehavior[] tiles;
        private BuildingComplexBehavior[] buildings;
        private SimpleCallback unlockedCallback;
        private bool isSubscribed;

        public bool IsUnlocked { get; private set; }

        public void Initialise(GroundTileComplexBehavior[] tiles, BuildingComplexBehavior[] buildings, SimpleCallback onUnlocked)
        {
            this.tiles = tiles;
            this.buildings = buildings;
            unlockedCallback = onUnlocked;

            IsUnlocked = false;
            isSubscribed = false;

            if (tiles.IsNullOrEmpty() && buildings.IsNullOrEmpty())
            {
                SetUnlocked();
                return;
            }

            Subscribe();
            Check();
        }

        public void Dispose()
        {
            Unsubscribe();
            unlockedCallback = null;
        }

        private void Subscribe()
        {
            isSubscribed = true;

            if (!tiles.IsNullOrEmpty())
            {
                foreach (var tile in tiles)
                {
                    if (tile == null)
                        continue;

                    tile.SubscribeOnFullyUnlocked(Check);
                    tile.InvokeOrSubscribe(Check);
                }
            }

            if (!buildings.IsNullOrEmpty())
            {
                foreach (var building in buildings)
                {
                    if (building == null)
                        continue;

                    building.SubscribeOnFullyUnlocked(Check);
                    building.InvokeOrSubscribe(Check);
                }
            }
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
                return;

            isSubscribed = false;

            if (!tiles.IsNullOrEmpty())
            {
                foreach (var tile in tiles)
                {
                    if (tile != null)
                        tile.UnsubscribeOnFullyUnlocked(Check);
                }
            }

            if (!buildings.IsNullOrEmpty())
            {
                foreach (var building in buildings)
                {
                    if (building != null)
                        building.UnsubscribeOnFullyUnlocked(Check);
                }
            }
        }

        private void Check()
        {
            if (!IsUnlocked && IsAnyElementOpen())
                SetUnlocked();
        }

        private bool IsAnyElementOpen()
        {
            if (!tiles.IsNullOrEmpty())
            {
                foreach (var tile in tiles)
                {
                    if (tile != null && tile.IsOpen)
                        return true;
                }
            }

            if (!buildings.IsNullOrEmpty())
            {
                foreach (var building in buildings)
                {
                    if (building != null && building.IsOpen)
                        return true;
                }
            }

            return false;
        }

        private void SetUnlocked()
        {
            IsUnlocked = true;

            Unsubscribe();

            var callback = unlockedCallback;
            callback?.Invoke();
        }
    }
}
