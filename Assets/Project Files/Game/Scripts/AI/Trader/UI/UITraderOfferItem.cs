using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UITraderOfferItem : MonoBehaviour
    {
        [SerializeField] Image receiveIcon;
        [SerializeField] TMP_Text receiveText;
        [SerializeField] TMP_Text descriptionText;

        [Space]
        [SerializeField] Button tradeButton;
        [SerializeField] TMP_Text buttonText;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField, Range(0f, 1f)] float unaffordableAlpha = 0.6f;

        private UITrader page;
        private WanderingTraderBehavior trader;
        private int offerIndex;

        public void Init(UITrader page, WanderingTraderBehavior trader, int offerIndex)
        {
            this.page = page;
            this.trader = trader;
            this.offerIndex = offerIndex;

            tradeButton.onClick.RemoveAllListeners();
            tradeButton.onClick.AddListener(OnTradeClicked);

            Build();
            Redraw();
        }

        private void Build()
        {
            var offer = trader.GetActiveOffer(offerIndex);
            if (offer == null)
                return;

            if (offer.Receive != null && offer.Receive.Length > 0 && receiveIcon != null)
            {
                var currency = CurrencyController.GetCurrency(offer.Receive[0].currency);
                if (currency != null)
                    receiveIcon.sprite = currency.Icon;
            }

            if (receiveText != null)
                receiveText.text = TraderResourceFormat.Format(offer.Receive);

            if (buttonText != null)
                buttonText.text = TraderResourceFormat.Format(offer.Give);
        }

        public void Redraw()
        {
            var remaining = trader.GetOfferRemaining(offerIndex);
            var canAfford = trader.CanAfford(offerIndex);

            if (descriptionText != null)
                descriptionText.text = "x" + remaining;

            tradeButton.interactable = remaining > 0 && canAfford;

            if (canvasGroup != null)
                canvasGroup.alpha = canAfford ? 1f : unaffordableAlpha;
        }

        private void OnTradeClicked()
        {
            if (trader.GetOfferRemaining(offerIndex) <= 0 || !trader.CanAfford(offerIndex))
                return;

            AudioController.PlaySound(AudioController.GetClip("button_sound"));

            page.PurchaseOffer(offerIndex);
        }
    }
}
