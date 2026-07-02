using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIBaseAttackPopup : MonoBehaviour
    {
        private const float CAMERA_FREEZE_TIME = 1.5f;

        [SerializeField] RectTransform panelRectTransform;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TextMeshProUGUI messageText;
        [SerializeField] Button button;

        [Space]
        [SerializeField] float slideOffset = 60f;
        [SerializeField] float duration = 0.35f;

        private Vector2 shownPosition;
        private Vector2 hiddenPosition;

        private BaseWorldBehavior subscribedWorld;
        private bool isShown;

        private TweenCase moveCase;
        private TweenCase fadeCase;

        public void Initialise()
        {
            if (panelRectTransform == null)
                panelRectTransform = (RectTransform)transform;

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (button == null)
                button = GetComponent<Button>();

            button.onClick.AddListener(OnClicked);

            shownPosition = panelRectTransform.anchoredPosition;
            hiddenPosition = shownPosition + Vector2.up * slideOffset;

            ApplyHiddenState();

            WorldController.OnWorldLoaded += OnWorldLoaded;

            SubscribeToCurrentWorld();
        }

        public void Unload()
        {
            WorldController.OnWorldLoaded -= OnWorldLoaded;
            UnsubscribeFromWorld();

            if (button != null)
                button.onClick.RemoveListener(OnClicked);

            moveCase.KillActive();
            fadeCase.KillActive();
        }

        private void OnClicked()
        {
            if (subscribedWorld == null || PreviewCamera.IsActive)
                return;

            var attackController = subscribedWorld.AttackController;
            if (attackController == null || !attackController.IsAlertActive)
                return;

            var player = PlayerBehavior.GetBehavior();
            if (player == null)
                return;

            var focusPosition = attackController.GetNearestDefensePosition(player.transform.position);

            AudioController.PlaySound(AudioController.GetClip("button_sound"));

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            PreviewCamera.Focus(focusPosition, CAMERA_FREEZE_TIME);
        }

        private void OnDestroy()
        {
            Unload();
        }

        private void OnWorldLoaded()
        {
            SubscribeToCurrentWorld();

            HideImmediately();
        }

        private void SubscribeToCurrentWorld()
        {
            var world = WorldController.WorldBehavior;
            if (world == subscribedWorld)
                return;

            UnsubscribeFromWorld();

            subscribedWorld = world;

            if (subscribedWorld != null)
            {
                subscribedWorld.BaseUnderAttack += Show;
                subscribedWorld.BaseAttackEnded += Hide;
            }
        }

        private void UnsubscribeFromWorld()
        {
            if (subscribedWorld == null)
                return;

            subscribedWorld.BaseUnderAttack -= Show;
            subscribedWorld.BaseAttackEnded -= Hide;
            subscribedWorld = null;
        }

        public void Show()
        {
            if (isShown)
                return;

            isShown = true;

            gameObject.SetActive(true);

            moveCase.KillActive();
            fadeCase.KillActive();

            panelRectTransform.anchoredPosition = hiddenPosition;
            canvasGroup.alpha = 0f;

            moveCase = panelRectTransform.DOAnchoredPosition(shownPosition, duration, unscaledTime: true).SetEasing(Ease.Type.BackOut);
            fadeCase = canvasGroup.DOFade(1f, duration, unscaledTime: true);
        }

        public void Hide()
        {
            if (!isShown)
                return;

            isShown = false;

            moveCase.KillActive();
            fadeCase.KillActive();

            moveCase = panelRectTransform.DOAnchoredPosition(hiddenPosition, duration, unscaledTime: true).SetEasing(Ease.Type.SineIn);
            fadeCase = canvasGroup.DOFade(0f, duration, unscaledTime: true).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }

        private void HideImmediately()
        {
            moveCase.KillActive();
            fadeCase.KillActive();

            ApplyHiddenState();
        }

        private void ApplyHiddenState()
        {
            isShown = false;
            canvasGroup.alpha = 0f;
            panelRectTransform.anchoredPosition = hiddenPosition;
            gameObject.SetActive(false);
        }
    }
}
