using UnityEngine;

namespace Watermelon
{
    public sealed class CrossWorldMissionHint
    {
        private const string MESSAGE = "Complete missions on another island";

        private static readonly Vector3 POINTER_POSITION_OFFSET = new Vector3(0, 4, 0);

        private readonly MissionUIPanel missionUIPanel;

        private PositionPointerCase positionPointerCase;

        private bool isShown;
        public bool IsShown => isShown;

        public CrossWorldMissionHint(MissionUIPanel missionUIPanel)
        {
            this.missionUIPanel = missionUIPanel;
        }

        public bool Show()
        {
            if (!HasDestination())
            {
                Hide();

                return false;
            }

            if (!isShown)
            {
                isShown = true;

                SubscribeSubworldEvents();

                missionUIPanel.ShowMessage(MESSAGE, OnHintClicked);
            }

            if (positionPointerCase == null)
                ActivatePositionPointer();

            return true;
        }

        public void Hide()
        {
            if (!isShown)
                return;

            isShown = false;

            UnsubscribeSubworldEvents();

            DisablePositionPointer();

            missionUIPanel.HideMessage();
        }

        public void Unload()
        {
            Hide();
        }

        private bool HasDestination()
        {
            var currentWorldID = WorldController.CurrentWorld != null ? WorldController.CurrentWorld.ID : string.Empty;

            return WorldMissionsProgress.TryGetWorldWithPendingMissions(currentWorldID, out _);
        }

        private void OnHintClicked()
        {
            if (PreviewCamera.IsActive)
                return;

            if (TryGetTargetPosition(out Vector3 targetPosition))
            {
                PreviewCamera.Focus(targetPosition, 1.5f);
            }
        }

        #region Navigation

        private void ActivatePositionPointer()
        {
            DisablePositionPointer();

            if (TryGetTargetPosition(out Vector3 targetPosition))
            {
                positionPointerCase = NavigationHelper.CreatePositionPointer(targetPosition + POINTER_POSITION_OFFSET);
                positionPointerCase.Show();
            }
        }

        private void DisablePositionPointer()
        {
            if (positionPointerCase == null)
                return;

            positionPointerCase.Disable();
            positionPointerCase = null;
        }

        private bool TryGetTargetPosition(out Vector3 position)
        {
            position = Vector3.zero;

            var worldBehavior = WorldController.WorldBehavior;

            if (worldBehavior == null)
                return false;

            var activeSubworld = worldBehavior.SubworldHandler.ActiveSubworld;

            if (activeSubworld != null)
            {
                if (activeSubworld.Exits.IsNullOrEmpty() || activeSubworld.Exits[0] == null)
                    return false;

                position = activeSubworld.Exits[0].transform.position;

                return true;
            }

            var playerBehavior = PlayerBehavior.GetBehavior();
            var origin = playerBehavior != null ? playerBehavior.transform.position : worldBehavior.transform.position;

            return worldBehavior.TryGetTravelPointPosition(origin, out position);
        }

        private void SubscribeSubworldEvents()
        {
            var subworldHandler = GetSubworldHandler();

            if (subworldHandler == null)
                return;

            subworldHandler.OnSubworldEnetered += OnSubworldEnteredOrLeft;
            subworldHandler.OnSubworldLeft += OnSubworldEnteredOrLeft;
        }

        private void UnsubscribeSubworldEvents()
        {
            var subworldHandler = GetSubworldHandler();

            if (subworldHandler == null)
                return;

            subworldHandler.OnSubworldEnetered -= OnSubworldEnteredOrLeft;
            subworldHandler.OnSubworldLeft -= OnSubworldEnteredOrLeft;
        }

        private SubworldHandler GetSubworldHandler()
        {
            var worldBehavior = WorldController.WorldBehavior;

            return worldBehavior != null ? worldBehavior.SubworldHandler : null;
        }

        private void OnSubworldEnteredOrLeft()
        {
            ActivatePositionPointer();
        }

        #endregion
    }
}
