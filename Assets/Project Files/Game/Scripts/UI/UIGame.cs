using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.GlobalUpgrades;
using Watermelon.IAPStore;

namespace Watermelon
{
    public class UIGame : UIPage
    {
        [SerializeField] RectTransform safeAreaRectTransform;
        [SerializeField] Joystick joystick;
        public Joystick Joystick => joystick;

        [SerializeField] CurrencyUIController currenciesUIController;
        public CurrencyUIController CurrenciesUIController => currenciesUIController;

        [SerializeField] MissionUIPanel missionUI;
        public MissionUIPanel MissionUIPanel => missionUI;

        [SerializeField] UIMissionRewardPopUp missionRewardPopUp;
        public UIMissionRewardPopUp MissionRewardPopUp => missionRewardPopUp;

        [SerializeField] EnergyUIPanel hungerUI;
        public EnergyUIPanel HungerUI => hungerUI;

        [SerializeField] UIWorldChangePopUp worldTransitionPopUp;
        public UIWorldChangePopUp WorldTransitionPopUp => worldTransitionPopUp;
        
        [Space]
        [SerializeField] Button upgradesButton;
        [SerializeField] Button iapStoreButton;
        [SerializeField] Button pauseButton;
        [SerializeField] TutorialCanvasController tutorialCanvasController;

        [Space]
        [SerializeField] RawImage backgroundImage;

        [Space]
        [SerializeField] GamepadIndicatorUI gamepadInteractor;

        [Header("Inventory")]
        [SerializeField] Button inventoryButton;
        [SerializeField] Image inventoryShine;
        [SerializeField] RectTransform inventoryRedDot;
        [SerializeField] TMP_Text inventoryCapacityText;
        [SerializeField] GameObject inventoryTutorial;

        private PlayerInventory playerInventory;
        private TweenCaseCollection caseCollection;
        private Coroutine shineCoroutine;

        public override void Init()
        {
            joystick.Init(canvas);

            inventoryButton.onClick.AddListener(OnInventoryButtonClicked);
            upgradesButton.onClick.AddListener(OnUpgradesButonClicked);
            iapStoreButton.onClick.AddListener(OnIAPStoreButtonClicked);
            pauseButton.onClick.AddListener(OnPauseButtonClicked);

            currenciesUIController.Init(CurrencyController.Currencies);

            if (EnergyController.IsEnergySystemEnabled)
            {
                hungerUI.Initialise();
            }
            else
            {
                hungerUI.gameObject.SetActive(false);
            }

            SafeAreaAdapter.RegisterRectTransform(safeAreaRectTransform);

            tutorialCanvasController.Init();
            worldTransitionPopUp.Initialise();

            backgroundImage.color = Color.white;

            gamepadInteractor.Init();
        }

        #region Show/Hide

        protected override void OnShow()
        {
            playerInventory = PlayerBehavior.GetBehavior().Inventory;
            playerInventory.CapacityChanged += UpdateInventoryUI;

            UpdateInventoryUI();

            NotifyOpened();

            UIGamepadButton.DisableAllTags();
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);
        }

        protected override void OnHide()
        {
            playerInventory.CapacityChanged -= UpdateInventoryUI;

            NotifyClosed();
        }

        #endregion

        #region Player Inventory

        private void OnInventoryButtonClicked()
        {
            UIController.ShowPage<InventoryUIPage>();

            InventoryUIPage inventoryUIPage = UIController.GetPage<InventoryUIPage>();
            inventoryUIPage.ActivateTutorial(inventoryTutorial.activeSelf);

            inventoryTutorial.SetActive(false);

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.GetClip("button_sound"));
        }

        private void UpdateInventoryUI()
        {
            if (!playerInventory.IsFull())
            {
                caseCollection.KillActive();
                caseCollection = Tween.BeginTweenCaseCollection();

                inventoryRedDot.DOScale(0, 0.2f).SetEasing(Ease.Type.SineIn);

                if (shineCoroutine != null)
                {
                    StopCoroutine(shineCoroutine);
                    shineCoroutine = null;

                    inventoryShine.DOFade(0, 0.2f);
                }

                inventoryCapacityText.text = CurrencyHelper.Format(playerInventory.CurrentCapacity) + "/" + CurrencyHelper.Format(playerInventory.MaxCapacity);

                Tween.EndTweenCaseCollection();
            }
            else
            {
                caseCollection.KillActive();
                caseCollection = Tween.BeginTweenCaseCollection();

                inventoryRedDot.DOScale(1, 0.2f).SetEasing(Ease.Type.SineOut);

                if (shineCoroutine == null)
                {
                    shineCoroutine = StartCoroutine(InventoryShineCoroutine());
                }

                inventoryCapacityText.text = "FULL!";

                Tween.EndTweenCaseCollection();
            }
        }

        private IEnumerator InventoryShineCoroutine()
        {
            float speed = 0.5f;

            while (true)
            {
                var alpha = inventoryShine.color.a;

                alpha += speed * Time.deltaTime;

                if (alpha > 1)
                {
                    alpha = 1;
                    speed *= -1;
                }
                else if (alpha <= 0)
                {
                    alpha = 0;
                    speed *= -1;
                }

                inventoryShine.SetAlpha(alpha);

                yield return null;
            }
        }

        public void SetInventoryTutorialState(bool state)
        {
            inventoryTutorial.SetActive(state);
        }
        #endregion

        public void SetBackgroundTexture(Texture texture)
        {
            backgroundImage.texture = texture;
        }

        private void OnUpgradesButonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.GetClip("button_sound"));

            GlobalUpgradesController.OpenMainUpgradesPage();

            StopUpgradesButtonHighlight();
        }

        private void OnIAPStoreButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.GetClip("button_sound"));

#if MODULE_MONETIZATION
            UIStore.OpenAsOverlay();
#endif
        }

        private void OnPauseButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.GetClip("button_sound"));

            UIController.ShowPage<UIPause>();
        }

        public void HighlightUpgradesButton()
        {
            RectTransform rectTransform = upgradesButton.transform as RectTransform;
            TutorialCanvasController.ActivatePointer(rectTransform.TransformPoint(rectTransform.rect.center), TutorialCanvasController.POINTER_CLICK);
        }

        public void StopUpgradesButtonHighlight()
        {
            TutorialCanvasController.ResetPointer();
        }
    }
}
