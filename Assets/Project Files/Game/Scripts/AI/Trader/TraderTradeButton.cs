using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class TraderTradeButton : MonoBehaviour, IDistanceToggle
    {
        [SerializeField] WorldSpaceButton worldSpaceButtonRef;
        [SerializeField] Canvas canvasRef;
        [SerializeField] float canvasHideDistance = 6f;

        public bool DistanceToggleActivated { get; private set; }
        public bool IsDistanceToggleInCloseMode { get; private set; }

        public float ActivationDistanceOfDT => canvasHideDistance;
        public Vector3 OriginPositionOfDT => transform.position;

        public event SimpleCallback Clicked;

        public bool IsInteractable { get; private set; } = true;

        private TweenCase canvasAppearCase;
        private Vector3 canvasDefaultScale;
        private bool isActive;

        private Transform cameraTransform;
        private Button button;

        private void Awake()
        {
            worldSpaceButtonRef.AddOnClickListener(OnButtonClicked);

            button = worldSpaceButtonRef.GetComponent<Button>();

            canvasDefaultScale = canvasRef.transform.localScale;
            canvasRef.enabled = false;
        }

        private void LateUpdate()
        {
            if (!isActive)
                return;

            if (cameraTransform == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera == null)
                    return;

                cameraTransform = mainCamera.transform;
            }

            transform.rotation = cameraTransform.rotation;
        }

        public void Activate()
        {
            if (isActive)
                return;

            isActive = true;
            DistanceToggleActivated = true;
            IsDistanceToggleInCloseMode = false;
            canvasRef.enabled = false;

            DistanceToggle.AddObject(this);
        }

        public void Deactivate()
        {
            if (!isActive)
                return;

            isActive = false;
            DistanceToggleActivated = false;
            IsDistanceToggleInCloseMode = false;

            canvasAppearCase.KillActive();
            canvasRef.enabled = false;

            DistanceToggle.RemoveObject(this);
        }

        public void SetInteractable(bool value)
        {
            if (IsInteractable == value)
                return;

            IsInteractable = value;

            if (button != null)
                button.interactable = value;
        }

        public void PlayerEnteredZone()
        {
            canvasRef.enabled = true;
            IsDistanceToggleInCloseMode = true;

            canvasAppearCase.KillActive();

            canvasRef.transform.localScale = canvasDefaultScale;
            canvasAppearCase = DistanceToggle.RunShowAnimation(canvasRef.transform);
        }

        public void PlayerLeavedZone()
        {
            IsDistanceToggleInCloseMode = false;

            canvasAppearCase.KillActive();
            canvasAppearCase = DistanceToggle.RunHideAnimation(canvasRef.transform, () =>
            {
                canvasRef.enabled = false;
            });
        }

        private void OnButtonClicked()
        {
            if (!isActive || !IsInteractable || !IsDistanceToggleInCloseMode)
                return;

            Clicked?.Invoke();
        }

        private void OnDisable()
        {
            if (isActive)
                DistanceToggle.RemoveObject(this);
        }
    }
}
