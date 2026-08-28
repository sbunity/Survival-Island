using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class MinigameStageAnchors
    {
        [SerializeField] Transform cameraAnchor;
        public Transform CameraAnchor => cameraAnchor;

        [SerializeField] Transform playerPoint;
        public Transform PlayerPoint => playerPoint;

        public bool IsValid => cameraAnchor != null && playerPoint != null;

        public MinigameStageAnchors() { }

        public MinigameStageAnchors(Transform cameraAnchor, Transform playerPoint)
        {
            this.cameraAnchor = cameraAnchor;
            this.playerPoint = playerPoint;
        }
    }

    public interface IMinigameStageProvider
    {
        bool TryGetStage(MinigameStageType stageType, out MinigameStageAnchors anchors);
    }
}
