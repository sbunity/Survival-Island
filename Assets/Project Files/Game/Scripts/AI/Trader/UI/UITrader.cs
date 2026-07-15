using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UITrader : UIPage
    {
        private readonly Vector2 DEFAULT_POSITION = new Vector2(0, 0);
        private readonly Vector2 HIDE_POSITION = new Vector2(0, -2000);

        [SerializeField] Image fadeImage;
        [SerializeField] RectTransform panelRectTransform;
        [SerializeField] RectTransform contentTransform;
        [SerializeField] Button closeButton;

        [Space]
        [SerializeField] GameObject offerItemPrefab;

        private UIGame mainPage;

        private WanderingTraderBehavior trader;
        public WanderingTraderBehavior CurrentTrader => trader;

        private readonly List<UITraderOfferItem> items = new List<UITraderOfferItem>();

        public override void Init()
        {
            mainPage = UIController.GetPage<UIGame>();

            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        public void SetTrader(WanderingTraderBehavior trader)
        {
            this.trader = trader;
        }

        protected override void OnShow()
        {
            mainPage.Joystick.HideVisuals();

            fadeImage.color = fadeImage.color.SetAlpha(0.0f);
            fadeImage.DOFade(0.25f, 0.4f);

            panelRectTransform.anchoredPosition = HIDE_POSITION;
            panelRectTransform.DOAnchoredPosition(DEFAULT_POSITION, 0.4f).SetEasing(Ease.Type.CircOut);

            RebuildItems();

            Control.DisableMovementControl();

            NotifyOpened();
        }

        protected override void OnHide()
        {
            if (trader != null)
                trader.OffersChanged -= OnOffersChanged;

            mainPage.Joystick.ShowVisuals();

            fadeImage.DOFade(0, 0.4f);
            panelRectTransform.DOAnchoredPosition(HIDE_POSITION, 0.4f).SetEasing(Ease.Type.CircIn).OnComplete(delegate
            {
                ClearItems();
                NotifyClosed();
            });

            Control.EnableMovementControl();
        }

        private void RebuildItems()
        {
            ClearItems();

            if (trader == null)
                return;

            trader.OffersChanged -= OnOffersChanged;
            trader.OffersChanged += OnOffersChanged;

            for (var i = 0; i < trader.ActiveOfferCount; i++)
            {
                if (trader.GetOfferRemaining(i) <= 0)
                    continue;

                var itemObject = Instantiate(offerItemPrefab, contentTransform);
                itemObject.SetActive(true);

                var item = itemObject.GetComponent<UITraderOfferItem>();
                item.Init(this, trader, i);

                items.Add(item);
            }
        }

        private void OnOffersChanged()
        {
            RebuildItems();
        }

        public void PurchaseOffer(int offerIndex)
        {
            if (trader != null)
                trader.TryPurchase(offerIndex);
        }

        private void ClearItems()
        {
            for (var i = 0; i < items.Count; i++)
                if (items[i] != null)
                    Destroy(items[i].gameObject);

            items.Clear();
        }

        private void OnCloseButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.GetClip("button_sound"));

            UIController.HidePage<UITrader>();
        }
    }
}
