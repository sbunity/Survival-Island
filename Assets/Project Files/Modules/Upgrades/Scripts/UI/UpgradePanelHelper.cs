using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    public class UpgradePanelHelper
    {
        private readonly List<UpgradeUIPanel> registeredPanels = new();
        private readonly List<IUpgrade> upgrades = new();

        private readonly List<UpgradeUIPanel> displayedPanels = new();

        public List<UpgradeUIPanel> UpgradeUIPanels => displayedPanels;
        public List<IUpgrade> Upgrades => upgrades;

        private Pool upgradesUIPool;
        public Pool UpgradesUIPool => upgradesUIPool;

        private IUpgradePanel panel;

        private bool isShown;

        private Vector2 firstSlotPosition;
        private float slotStep;
        private bool areSlotsCaptured;

        public event SimpleBoolCallback OrderChanged;

        public UpgradePanelHelper(IUpgradePanel panel)
        {
            this.panel = panel;

            upgradesUIPool = new Pool(panel.UpgradeUIPrefab, panel.UpgradeUIPrefab.name, panel.ContentTransform);
        }

        public void Unload()
        {
            Reset();

            upgradesUIPool?.Destroy();
        }

        public void AddUpgrades(List<IUpgrade> upgrades)
        {
            for (int i = 0; i < upgrades.Count; i++)
            {
                AddUpgrade(upgrades[i]);
            }
        }

        public void AddUpgrade(IUpgrade upgrade)
        {
            GameObject upgradeUIObject = UpgradesUIPool.GetPooledObject();
            upgradeUIObject.transform.ResetLocal();
            upgradeUIObject.SetActive(true);

            UpgradeUIPanel upgradeUIPanel = upgradeUIObject.GetComponent<UpgradeUIPanel>();
            upgradeUIPanel.Initialise(upgrade);

            registeredPanels.Add(upgradeUIPanel);
            displayedPanels.Add(upgradeUIPanel);
            upgrades.Add(upgrade);

            upgrade.HighlightChanged += OnHighlightChanged;
        }

        public void Reset()
        {
            Hide();

            for (var i = 0; i < upgrades.Count; i++)
            {
                upgrades[i].HighlightChanged -= OnHighlightChanged;
            }

            for (var i = 0; i < registeredPanels.Count; i++)
            {
                registeredPanels[i].Disable();
            }

            upgradesUIPool.ReturnToPoolEverything();

            registeredPanels.Clear();
            displayedPanels.Clear();
            upgrades.Clear();

            areSlotsCaptured = false;
        }

        public void Show()
        {
            isShown = true;

            for (var i = 0; i < registeredPanels.Count; i++)
            {
                registeredPanels[i].gameObject.SetActive(true);
            }

            RebuildDisplayedOrder();

            ApplyOrder(false);
        }

        public void Hide()
        {
            isShown = false;
        }

        public void CaptureSlots()
        {
            var layoutGroup = panel.ContentLayoutGroup;

            if (layoutGroup == null || displayedPanels.Count == 0)
                return;

            layoutGroup.enabled = true;

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)panel.ContentTransform);

            firstSlotPosition = displayedPanels[0].Rect.anchoredPosition;

            slotStep = displayedPanels.Count > 1
                ? firstSlotPosition.y - displayedPanels[1].Rect.anchoredPosition.y
                : displayedPanels[0].Height + layoutGroup.spacing;

            areSlotsCaptured = true;

            layoutGroup.enabled = false;
        }

        public void Redraw(bool animation)
        {
            for (var i = 0; i < displayedPanels.Count; i++)
            {
                displayedPanels[i].Redraw();

                if (animation)
                {
                    var index = i;

                    displayedPanels[i].CanvasGroup.alpha = 0.0f;

                    Tween.DelayedCall(0.2f + i * 0.09f, delegate
                    {
                        displayedPanels[index].CanvasGroup.DOFade(1.0f, 0.6f).SetEasing(Ease.Type.SineOut);
                    });
                }
                else
                {
                    displayedPanels[i].CanvasGroup.alpha = 1.0f;
                }
            }
        }

        public void OnUpgraded(GlobalUpgradeType upgradeType, AbstactGlobalUpgrade upgrade)
        {
            if (panel.ShowAllAfterUpgrade)
            {
                for (var i = 0; i < registeredPanels.Count; i++)
                {
                    registeredPanels[i].gameObject.SetActive(true);
                }

                panel.ShowAllAfterUpgrade = false;

                RebuildDisplayedOrder();
                ApplyOrder(false);
            }

            for (var i = 0; i < registeredPanels.Count; i++)
            {
                registeredPanels[i].Redraw();
            }
        }

        private void OnHighlightChanged()
        {
            if (!isShown)
                return;

            RebuildDisplayedOrder();

            ApplyOrder(true);
        }

        private void RebuildDisplayedOrder()
        {
            displayedPanels.Clear();

            for (var i = 0; i < registeredPanels.Count; i++)
            {
                if (registeredPanels[i].Upgrade.IsHighlighted)
                    displayedPanels.Add(registeredPanels[i]);
            }

            for (var i = 0; i < registeredPanels.Count; i++)
            {
                if (!registeredPanels[i].Upgrade.IsHighlighted)
                    displayedPanels.Add(registeredPanels[i]);
            }
        }

        private void ApplyOrder(bool animated)
        {
            for (var i = 0; i < displayedPanels.Count; i++)
            {
                var upgradePanel = displayedPanels[i];

                upgradePanel.transform.SetSiblingIndex(i);

                var isHighlighted = upgradePanel.Upgrade.IsHighlighted;
                var color = isHighlighted ? panel.HighlightedColor : panel.DefaultColor;

                var slotPosition = areSlotsCaptured ? GetSlotPosition(i) : upgradePanel.Rect.anchoredPosition;

                upgradePanel.ApplySlot(slotPosition, color, isHighlighted, animated && areSlotsCaptured);
            }

            OrderChanged?.Invoke(animated && areSlotsCaptured);
        }

        private Vector2 GetSlotPosition(int index)
        {
            return new Vector2(firstSlotPosition.x, firstSlotPosition.y - slotStep * index);
        }
    }
}
