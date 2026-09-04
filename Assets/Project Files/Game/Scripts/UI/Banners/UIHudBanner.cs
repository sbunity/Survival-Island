using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public abstract class UIHudBanner : MonoBehaviour
    {
        private const float CAMERA_FREEZE_TIME = 1.5f;

        [SerializeField] HudBannerPriority priority;

        [Space]
        [SerializeField] RectTransform panelRectTransform;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Button button;

        [Space]
        [SerializeField] float slideOffset = 60f;
        [SerializeField] float duration = 0.35f;
        [SerializeField] float reflowDuration = 0.25f;

        public HudBannerPriority Priority => priority;

        public bool IsShown { get; private set; }

        public float Height => panelRectTransform != null ? panelRectTransform.rect.height : 0f;

        public event SimpleCallback VisibilityChanged;

        private Vector2 slotPosition;
        private bool isAppearing;

        private TweenCase moveCase;
        private TweenCase fadeCase;

        private bool isInitialised;

        public void Initialise()
        {
            if (isInitialised)
                return;

            if (panelRectTransform == null)
                panelRectTransform = (RectTransform)transform;

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (button == null)
                button = GetComponent<Button>();

            if (button != null)
                button.onClick.AddListener(OnButtonClicked);

            slotPosition = panelRectTransform.anchoredPosition;

            ApplyHiddenState();

            isInitialised = true;

            OnInitialise();
        }

        public void Unload()
        {
            if (!isInitialised)
                return;

            isInitialised = false;

            OnUnload();

            if (button != null)
                button.onClick.RemoveListener(OnButtonClicked);

            moveCase.KillActive();
            fadeCase.KillActive();
        }

        private void OnDestroy()
        {
            Unload();
        }

        protected abstract void OnInitialise();

        protected abstract void OnUnload();

        protected abstract void OnClicked();

        protected void Show()
        {
            if (!isInitialised || IsShown)
                return;

            IsShown = true;
            isAppearing = true;

            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;

            // the stack hands out the slot before the intro animation starts
            VisibilityChanged?.Invoke();

            isAppearing = false;

            moveCase.KillActive();
            fadeCase.KillActive();

            panelRectTransform.anchoredPosition = slotPosition + Vector2.up * slideOffset;

            moveCase = panelRectTransform.DOAnchoredPosition(slotPosition, duration, unscaledTime: true).SetEasing(Ease.Type.BackOut);
            fadeCase = canvasGroup.DOFade(1f, duration, unscaledTime: true);
        }

        protected void Hide()
        {
            if (!isInitialised || !IsShown)
                return;

            IsShown = false;

            VisibilityChanged?.Invoke();

            moveCase.KillActive();
            fadeCase.KillActive();

            var hiddenPosition = panelRectTransform.anchoredPosition + Vector2.up * slideOffset;

            moveCase = panelRectTransform.DOAnchoredPosition(hiddenPosition, duration, unscaledTime: true).SetEasing(Ease.Type.SineIn);
            fadeCase = canvasGroup.DOFade(0f, duration, unscaledTime: true).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }

        protected void HideImmediately()
        {
            if (!IsShown)
                return;

            IsShown = false;

            moveCase.KillActive();
            fadeCase.KillActive();

            ApplyHiddenState();

            VisibilityChanged?.Invoke();
        }

        public void ApplySlot(float anchoredY, bool animated)
        {
            slotPosition = new Vector2(slotPosition.x, anchoredY);

            if (isAppearing || !IsShown)
                return;

            moveCase.KillActive();

            if (!animated)
            {
                panelRectTransform.anchoredPosition = slotPosition;

                return;
            }

            moveCase = panelRectTransform.DOAnchoredPosition(slotPosition, reflowDuration, unscaledTime: true).SetEasing(Ease.Type.CubicOut);
        }

        protected static void FocusCamera(Vector3 position)
        {
            if (PreviewCamera.IsActive)
                return;

            AudioController.PlaySound(AudioController.GetClip("button_sound"));

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            PreviewCamera.Focus(position, CAMERA_FREEZE_TIME);
        }

        private void ApplyHiddenState()
        {
            canvasGroup.alpha = 0f;
            panelRectTransform.anchoredPosition = slotPosition + Vector2.up * slideOffset;

            gameObject.SetActive(false);
        }

        private void OnButtonClicked()
        {
            if (!isInitialised || !IsShown)
                return;

            OnClicked();
        }
    }
}
