using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

#if MODULE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace Watermelon
{
    // Runtime dev cheat panel — lets resources be added while testing.
    // Built with regular uGUI (not OnGUI/IMGUI) so it works correctly with the
    // project's "Input System Package (New)" only input handling, on both
    // desktop and touch/mobile devices.
    // Only exists in the Editor or Development Builds, never in a release build.
    public class CheatManager : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const KeyCode LegacyToggleKey = KeyCode.BackQuote;

        private const int LargeAmount = 100;
        private const int HugeAmount = 1000;

        private const int ReferenceWidth = 1080;
        private const int ReferenceHeight = 1920;

        private static readonly Color PanelColor = new Color(0.05f, 0.05f, 0.07f, 0.93f);
        private static readonly Color ButtonColor = new Color(0.20f, 0.45f, 0.85f, 1f);
        private static readonly Color AddAllButtonColor = new Color(0.20f, 0.65f, 0.35f, 1f);

        private GameObject panelObject;
        private bool isPanelVisible;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            GameObject cheatManagerObject = new GameObject("Cheat Manager");
            cheatManagerObject.AddComponent<CheatManager>();

            DontDestroyOnLoad(cheatManagerObject);
        }

        private void Start()
        {
            EnsureEventSystem();
            BuildUI();
        }

        private void Update()
        {
#if MODULE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame)
            {
                TogglePanel();
            }
#else
            if (Input.GetKeyDown(LegacyToggleKey))
            {
                TogglePanel();
            }
#endif
        }

        private void TogglePanel()
        {
            isPanelVisible = !isPanelVisible;
            panelObject.SetActive(isPanelVisible);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            GameObject eventSystemObject = new GameObject("EventSystem (Cheats)");

#if MODULE_INPUT_SYSTEM
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif

            DontDestroyOnLoad(eventSystemObject);
        }

        private void BuildUI()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            RectTransform toggleButton = CreateButton(transform, "Toggle Button", "CHEATS", TogglePanel, 260, 130, 34, ButtonColor);
            toggleButton.anchorMin = new Vector2(1, 0);
            toggleButton.anchorMax = new Vector2(1, 0);
            toggleButton.pivot = new Vector2(1, 0);
            toggleButton.anchoredPosition = new Vector2(-24, 24);

            panelObject = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            panelObject.transform.SetParent(transform, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 0.5f);
            panelRect.sizeDelta = new Vector2(720, -48);
            panelRect.anchoredPosition = new Vector2(24, 0);

            panelObject.GetComponent<Image>().color = PanelColor;
            panelObject.SetActive(false);

            VerticalLayoutGroup panelLayout = panelObject.GetComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(24, 24, 24, 24);
            panelLayout.spacing = 16;
            panelLayout.childControlHeight = true;
            panelLayout.childControlWidth = true;
            panelLayout.childForceExpandHeight = false;
            panelLayout.childForceExpandWidth = true;

            CreateText(panelRect, "Title", "CHEATS", 48, TextAlignmentOptions.Center, 70);

            RectTransform scrollViewRect = CreateScrollView(panelRect, out RectTransform content);
            LayoutElement scrollLayoutElement = scrollViewRect.gameObject.AddComponent<LayoutElement>();
            scrollLayoutElement.flexibleHeight = 1;
            scrollLayoutElement.minHeight = 200;

            foreach (CurrencyType currencyType in Enum.GetValues(typeof(CurrencyType)))
            {
                CreateResourceRow(content, currencyType);
            }

            RectTransform addAllButton = CreateButton(panelRect, "Add All Button", "ADD " + LargeAmount + " OF EVERYTHING", () =>
            {
                foreach (CurrencyType currencyType in Enum.GetValues(typeof(CurrencyType)))
                {
                    CurrencyController.Add(currencyType, LargeAmount, "cheat");
                }
            }, 0, 110, 30, AddAllButtonColor, expandWidth: true);
        }

        private RectTransform CreateScrollView(Transform parent, out RectTransform content)
        {
            GameObject scrollObject = new GameObject("Scroll View", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(parent, false);
            scrollObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.15f);

            GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportObject.transform.SetParent(scrollObject.transform, false);
            RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportObject.GetComponent<Image>().color = Color.white;
            viewportObject.GetComponent<Mask>().showMaskGraphic = false;

            GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.transform.SetParent(viewportRect, false);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup contentLayout = contentObject.GetComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 12;
            contentLayout.childControlHeight = true;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;

            ContentSizeFitter contentFitter = contentObject.GetComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            content = contentRect;
            return scrollObject.GetComponent<RectTransform>();
        }

        private void CreateResourceRow(Transform parent, CurrencyType currencyType)
        {
            GameObject rowObject = new GameObject(currencyType + " Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            rowObject.transform.SetParent(parent, false);

            HorizontalLayoutGroup rowLayout = rowObject.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 12;
            rowLayout.childControlHeight = true;
            rowLayout.childControlWidth = true;
            rowLayout.childForceExpandHeight = true;
            rowLayout.childForceExpandWidth = false;

            LayoutElement rowLayoutElement = rowObject.GetComponent<LayoutElement>();
            rowLayoutElement.minHeight = 96;
            rowLayoutElement.preferredHeight = 96;

            RectTransform labelRect = CreateText(rowObject.transform, "Label", currencyType.ToString(), 30, TextAlignmentOptions.MidlineLeft, 0);
            LayoutElement labelLayoutElement = labelRect.gameObject.AddComponent<LayoutElement>();
            labelLayoutElement.minWidth = 220;
            labelLayoutElement.flexibleWidth = 1;

            CreateButton(rowObject.transform, currencyType + " +" + LargeAmount, "+" + LargeAmount,
                () => CurrencyController.Add(currencyType, LargeAmount, "cheat"), 160, 96, 28, ButtonColor);

            CreateButton(rowObject.transform, currencyType + " +" + HugeAmount, "+" + HugeAmount,
                () => CurrencyController.Add(currencyType, HugeAmount, "cheat"), 160, 96, 28, ButtonColor);
        }

        private RectTransform CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment, float preferredHeight)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            if (preferredHeight > 0)
            {
                LayoutElement layoutElement = textObject.AddComponent<LayoutElement>();
                layoutElement.preferredHeight = preferredHeight;
            }

            return textObject.GetComponent<RectTransform>();
        }

        private RectTransform CreateButton(Transform parent, string name, string label, Action onClick, float width, float height, int fontSize, Color color, bool expandWidth = false)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            if (!expandWidth)
            {
                rect.sizeDelta = new Vector2(width, height);
            }

            buttonObject.GetComponent<Image>().color = color;
            buttonObject.GetComponent<Button>().onClick.AddListener(() => onClick());

            LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = height;
            if (expandWidth)
            {
                layoutElement.flexibleWidth = 1;
            }
            else
            {
                layoutElement.preferredWidth = width;
            }

            GameObject labelObject = new GameObject("Label", typeof(RectTransform));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = labelObject.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            return rect;
        }
#endif
    }
}
