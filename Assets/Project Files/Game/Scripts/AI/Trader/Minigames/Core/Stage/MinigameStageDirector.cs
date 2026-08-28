using System.Collections;
using UnityEngine;

namespace Watermelon
{
    public class MinigameStageDirector
    {
        private MinigameStageType activeStage = MinigameStageType.None;

        private Coroutine waitCoroutine;
        private SimpleCallback pendingCallback;

        public bool IsActive => activeStage != MinigameStageType.None;

        public void Enter(MinigameStageType stageType, MinigameStageAnchors anchors, SimpleCallback onReady)
        {
            StopWaiting();

            if (!TryResolveCamera(stageType, anchors, out var cameraType))
            {
                onReady?.Invoke();

                return;
            }

            activeStage = stageType;

            var player = PlayerBehavior.GetBehavior();
            if (player != null)
                player.Warp(anchors.PlayerPoint);

            var camera = CameraController.GetCamera(cameraType);
            camera.SetTarget(anchors.CameraAnchor);

            CameraController.EnableCamera(cameraType);

            WaitForBlend(camera, onReady);
        }

        public void Exit(SimpleCallback onComplete)
        {
            StopWaiting();

            if (!IsActive)
            {
                onComplete?.Invoke();

                return;
            }

            activeStage = MinigameStageType.None;

            CameraController.EnableCamera(CameraType.Gameplay);

            WaitForBlend(CameraController.GetCamera(CameraType.Gameplay), onComplete);
        }

        public void Abort()
        {
            StopWaiting();

            if (!IsActive)
                return;

            activeStage = MinigameStageType.None;

            CameraController.EnableCamera(CameraType.Gameplay);
        }

        private static bool TryResolveCamera(MinigameStageType stageType, MinigameStageAnchors anchors, out CameraType cameraType)
        {
            cameraType = CameraType.Gameplay;

            if (stageType == MinigameStageType.None)
                return false;

            if (anchors == null || !anchors.IsValid)
            {
                Debug.LogError($"[Trader Minigames]: stage \"{stageType}\" has no anchors, playing without the camera move.");

                return false;
            }

            if (!TryGetCameraType(stageType, out cameraType))
                return false;

            if (!CameraController.HasCamera(cameraType))
            {
                Debug.LogError($"[Trader Minigames]: no {cameraType} virtual camera in the scene, playing without the camera move.");

                return false;
            }

            return true;
        }

        private static bool TryGetCameraType(MinigameStageType stageType, out CameraType cameraType)
        {
            switch (stageType)
            {
                case MinigameStageType.Floor:
                    cameraType = CameraType.MinigameFloor;

                    return true;

                case MinigameStageType.Table:
                    cameraType = CameraType.MinigameTable;

                    return true;

                default:
                    cameraType = CameraType.Gameplay;

                    return false;
            }
        }

        private void WaitForBlend(VirtualCamera camera, SimpleCallback onComplete)
        {
            pendingCallback = onComplete;

            waitCoroutine = Tween.InvokeCoroutine(WaitForBlendRoutine(camera));

            if (waitCoroutine == null)
                InvokePending();
        }

        private IEnumerator WaitForBlendRoutine(VirtualCamera camera)
        {
            yield return null;

            while (camera != null && camera.IsBlending)
                yield return null;

            waitCoroutine = null;

            InvokePending();
        }

        private void InvokePending()
        {
            var callback = pendingCallback;
            pendingCallback = null;

            callback?.Invoke();
        }

        private void StopWaiting()
        {
            if (waitCoroutine != null)
            {
                Tween.StopCustomCoroutine(waitCoroutine);

                waitCoroutine = null;
            }

            pendingCallback = null;
        }
    }
}
