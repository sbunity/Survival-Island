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
        [SerializeField] GameObject minigameItemPrefab;

        private WanderingTraderBehavior trader;
        public WanderingTraderBehavior CurrentTrader => trader;

        private readonly List<UITraderOfferItem> items = new List<UITraderOfferItem>();
        private UITraderMinigameItem minigameItem;

        private WanderingTraderBehavior subscribedTrader;

        private bool holdsMovementLock;

        public override void Init()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        public void SetTrader(WanderingTraderBehavior trader)
        {
            this.trader = trader;
        }

        protected override void OnShow()
        {
            AcquireMovementLock();

            fadeImage.color = fadeImage.color.SetAlpha(0.0f);
            fadeImage.DOFade(0.25f, 0.4f);

            panelRectTransform.anchoredPosition = HIDE_POSITION;
            panelRectTransform.DOAnchoredPosition(DEFAULT_POSITION, 0.4f).SetEasing(Ease.Type.CircOut);

            Subscribe();
            BuildItems();

            NotifyOpened();
        }

        protected override void OnHide()
        {
            Unsubscribe();

            fadeImage.DOFade(0, 0.4f);
            panelRectTransform.DOAnchoredPosition(HIDE_POSITION, 0.4f).SetEasing(Ease.Type.CircIn).OnComplete(delegate
            {
                ClearItems();
                NotifyClosed();
            });

            ReleaseMovementLock();
        }

        protected override void OnUnload()
        {
            Unsubscribe();
            ReleaseMovementLock();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            ReleaseMovementLock();
        }

        private void AcquireMovementLock()
        {
            if (holdsMovementLock)
                return;

            holdsMovementLock = true;

            MovementLock.Acquire();
        }

        private void ReleaseMovementLock()
        {
            if (!holdsMovementLock)
                return;

            holdsMovementLock = false;

            MovementLock.Release();
        }

        #region Trader Subscription
        private void Subscribe()
        {
            if (subscribedTrader == trader)
                return;

            Unsubscribe();

            if (trader == null)
                return;

            subscribedTrader = trader;

            subscribedTrader.OffersChanged += OnTraderChanged;
            subscribedTrader.MinigameChanged += OnTraderChanged;
        }

        private void Unsubscribe()
        {
            if (subscribedTrader == null)
                return;

            subscribedTrader.OffersChanged -= OnTraderChanged;
            subscribedTrader.MinigameChanged -= OnTraderChanged;

            subscribedTrader = null;
        }
        #endregion

        #region Items
        private void OnTraderChanged()
        {
            if (HasLayoutChanged())
                BuildItems();
            else
                RedrawItems();
        }

        private bool HasLayoutChanged()
        {
            if (trader == null)
                return items.Count > 0 || minigameItem != null;

            if (HasMinigame() != (minigameItem != null))
                return true;

            var shownCount = 0;

            for (var i = 0; i < trader.ActiveOfferCount; i++)
            {
                if (trader.GetOfferRemaining(i) <= 0)
                    continue;

                if (shownCount >= items.Count || items[shownCount] == null || items[shownCount].OfferIndex != i)
                    return true;

                shownCount++;
            }

            return shownCount != items.Count;
        }

        private bool HasMinigame()
        {
            if (minigameItemPrefab == null || trader == null)
                return false;

            var slot = trader.MinigameSlot;

            return slot != null && slot.HasGame;
        }

        private void BuildItems()
        {
            ClearItems();

            if (trader == null)
                return;

            if (HasMinigame())
            {
                minigameItem = CreateItem<UITraderMinigameItem>(minigameItemPrefab);
                minigameItem.Init(this, trader.MinigameSlot);
                minigameItem.gameObject.SetActive(true);
            }

            for (var i = 0; i < trader.ActiveOfferCount; i++)
            {
                if (trader.GetOfferRemaining(i) <= 0)
                    continue;

                var item = CreateItem<UITraderOfferItem>(offerItemPrefab);
                item.Init(this, trader, i);
                item.gameObject.SetActive(true);

                items.Add(item);
            }
        }

        private void RedrawItems()
        {
            if (minigameItem != null)
                minigameItem.Redraw();

            for (var i = 0; i < items.Count; i++)
                if (items[i] != null)
                    items[i].Redraw();
        }

        private T CreateItem<T>(GameObject prefab) where T : Component
        {
            var itemObject = Instantiate(prefab, contentTransform);
            itemObject.SetActive(false);

            return itemObject.GetComponent<T>();
        }

        private void ClearItems()
        {
            if (minigameItem != null)
            {
                DestroyItem(minigameItem.gameObject);

                minigameItem = null;
            }

            for (var i = 0; i < items.Count; i++)
                if (items[i] != null)
                    DestroyItem(items[i].gameObject);

            items.Clear();
        }

        private static void DestroyItem(GameObject itemObject)
        {
            itemObject.SetActive(false);

            Destroy(itemObject);
        }
        #endregion

        public void PurchaseOffer(int offerIndex)
        {
            if (trader != null)
                trader.TryPurchase(offerIndex);
        }

        public void PlayMinigame()
        {
            if (trader != null)
                trader.PlayMinigame();
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
