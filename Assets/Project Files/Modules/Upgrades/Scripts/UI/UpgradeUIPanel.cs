using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    public class UpgradeUIPanel : MonoBehaviour
    {
        private const string LEVEL = "LVL {0}";

        private const float SLIDE_DURATION = 0.35f;
        private const float COLOR_DURATION = 0.25f;
        private const float POP_SCALE = 1.05f;

        [SerializeField] RectTransform rect;
        public float Height => rect.sizeDelta.y;

        [Space]
        [SerializeField] Image backgroundImage;
        [SerializeField] Image iconImage;
        [SerializeField] Image buttonImage;
        [SerializeField] Button upgradeButton;
        [SerializeField] Sprite buttonActiveSprite;
        [SerializeField] Sprite buttonDisableSprite;

        [Space]
        [SerializeField] GameObject adsButtonObject;

        [Space]
        [SerializeField] GameObject maxObject;

        [Space]
        [SerializeField] TextMeshProUGUI priceText;
        [SerializeField] TextMeshProUGUI levelText;
        [SerializeField] TextMeshProUGUI nameText;

        [Space]
        [SerializeField] TextMeshProUGUI descriptionText;

        [Space]
        [SerializeField] Image outline;

        public bool IsSelected { get; private set; }

        public RectTransform Rect { get; private set; }

        public Image BackgroundImage => backgroundImage;

        private CanvasGroup canvasGroup;
        public CanvasGroup CanvasGroup => canvasGroup;

        private CanvasGroup priceCanvasGroup;

        private IUpgrade upgrade;
        public IUpgrade Upgrade => upgrade;

        private Currency currency;

        private bool isHighlighted;

        private TweenCase slideCase;
        private TweenCase colorCase;
        private TweenCase popCase;

        private void Awake()
        {
            upgradeButton.onClick.AddListener(OnPurchaseButtonClicked);

            outline.enabled = false;

            Rect = GetComponent<RectTransform>();
        }

        public void OnSelect()
        {
            IsSelected = true;

            if (Control.InputType == InputType.Gamepad)
            {
                outline.enabled = true;
            }

            Control.OnInputChanged += OnControlChanged;
        }

        public void OnDeselect()
        {
            IsSelected = false;

            outline.enabled = false;

            Control.OnInputChanged -= OnControlChanged;
        }

        private void OnControlChanged(InputType type)
        {
            outline.enabled = type == InputType.Gamepad && IsSelected;
        }

        private void Update()
        {
            if (IsSelected && Control.InputType == InputType.Gamepad)
            {
                if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.A))
                {
                    upgradeButton.ClickButton();
                }
            }
        }

        public void Initialise(IUpgrade upgrade)
        {
            this.upgrade = upgrade;

            // Get component
            canvasGroup = GetComponent<CanvasGroup>();
            priceCanvasGroup = priceText.GetComponent<CanvasGroup>();

            KillVisualTweens();

            isHighlighted = upgrade.IsHighlighted;
            transform.localScale = Vector3.one;

            // Redraw panel
            Redraw();

            // Set name
            nameText.text = upgrade.Title;
            iconImage.sprite = upgrade.Icon;

            if (currency != null) currency.OnCurrencyChanged -= OnCurrencyAmountChanged;

            currency = CurrencyController.GetCurrency(upgrade.GetUpgradeCost(upgrade.UpgradeIndex).currency);
            currency.OnCurrencyChanged += OnCurrencyAmountChanged;
        }

        private void OnCurrencyAmountChanged(Currency currency, int difference)
        {
            Redraw();
        }

        public void Redraw()
        {
            // Upgrade is maxed out
            if (upgrade.IsMaxedOut)
            {
                // Disable button
                buttonImage.gameObject.SetActive(false);
                adsButtonObject.gameObject.SetActive(false);

                // Enable hired panel
                maxObject.SetActive(true);
                descriptionText.text = string.Empty;
                descriptionText.gameObject.SetActive(true);

                // Reset panel transparent
                canvasGroup.alpha = 1.0f;
            }
            else
            {
                // Enable button
                buttonImage.gameObject.SetActive(true);
                adsButtonObject.gameObject.SetActive(true);

                // Enable hired panel
                maxObject.SetActive(false);


                Resource price = upgrade.GetUpgradeCost(upgrade.UpgradeIndex + 1);
                if (CurrencyController.HasAmount(price.currency, price.amount))
                {
                    buttonImage.sprite = buttonActiveSprite;
                    priceCanvasGroup.alpha = 1.0f;
                }
                else
                {
                    buttonImage.sprite = buttonDisableSprite;
                    priceCanvasGroup.alpha = 0.6f;
                }

                // Reset panel transparent
                canvasGroup.alpha = 1.0f;

                // Set price
                priceText.text = $"{CurrencyHelper.Format(price.amount)} <sprite name={price.currency}>";
                descriptionText.gameObject.SetActive(true);
                descriptionText.text = Upgrade.GetUpgradeDescription(upgrade.UpgradeIndex);
            }

            // Set level
            levelText.text = string.Format(LEVEL, upgrade.UpgradeIndex + 1);
        }

        public void OnPurchaseButtonClicked()
        {
            if (!upgrade.IsMaxedOut)
            {
                Resource price = upgrade.GetUpgradeCost(upgrade.UpgradeIndex + 1);
                if (CurrencyController.HasAmount(price.currency, price.amount))
                {
                    AudioController.PlaySound(AudioController.GetClip("button_sound"));

                    CurrencyController.Substract(price.currency, price.amount);

                    upgrade.UpgradeStage();
                    Redraw();
                }
            }
        }

        public void OnPurchaseAdButtonClicked()
        {
            if (!upgrade.IsMaxedOut)
            {
                AudioController.PlaySound(AudioController.GetClip("button_sound"));

#if MODULE_ADS
                AdsManager.ShowRewardBasedVideo((reward) =>
                {
                    if (reward)
                    {
                        upgrade.UpgradeStage();
                    }
                });
#else
                upgrade.UpgradeStage();

                Debug.LogWarning("Ads module is missing - granting the upgrade without an ad!");
#endif
            }
        }

        public void ApplySlot(Vector2 slotPosition, Color color, bool highlighted, bool animated)
        {
            slideCase.KillActive();
            colorCase.KillActive();

            var becameHighlighted = highlighted && !isHighlighted;

            isHighlighted = highlighted;

            if (!animated)
            {
                popCase.KillActive();

                transform.localScale = Vector3.one;
                Rect.anchoredPosition = slotPosition;
                backgroundImage.color = color;

                return;
            }

            if (Rect.anchoredPosition != slotPosition)
                slideCase = Rect.DOAnchoredPosition(slotPosition, SLIDE_DURATION).SetEasing(Ease.Type.CubicOut);

            if (backgroundImage.color != color)
                colorCase = backgroundImage.DOColor(color, COLOR_DURATION).SetEasing(Ease.Type.SineOut);

            if (becameHighlighted)
            {
                popCase.KillActive();

                transform.localScale = Vector3.one;
                popCase = transform.DOPushScale(Vector3.one * POP_SCALE, Vector3.one, 0.16f, 0.24f, Ease.Type.SineOut, Ease.Type.SineInOut);
            }
        }

        private void KillVisualTweens()
        {
            slideCase.KillActive();
            colorCase.KillActive();
            popCase.KillActive();
        }

        public void Disable()
        {
            if (currency != null) currency.OnCurrencyChanged -= OnCurrencyAmountChanged;
            currency = null;

            KillVisualTweens();

            transform.localScale = Vector3.one;

            OnDeselect();
        }
    }
}