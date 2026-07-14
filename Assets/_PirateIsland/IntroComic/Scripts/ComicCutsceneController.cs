using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ComicCutsceneController : MonoBehaviour
{
    [Serializable]
    public enum ComicTextType { Sound, Dialogue, Narration }

    [Serializable]
    public class ComicTextEntry
    {
        public TypewriterText target;
        public ComicTextType type;
        [TextArea(1, 3)] public string text;
        public float delayBefore;
        public bool showInstantly;
    }

    [Serializable]
    public class ComicPanelData
    {
        public CanvasGroup panel;
        public List<ComicTextEntry> texts = new List<ComicTextEntry>();
    }

    [Header("Comic panels in display order")]
    [SerializeField] private List<ComicPanelData> panels = new List<ComicPanelData>();

    [Header("Controls")]
    [SerializeField] private Button skipButton;
    [SerializeField] private Button continueButton;

    [Header("Animation")]
    [SerializeField] private float delayBetweenPanels = 0.25f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.96f, 0.96f, 1f);
    [SerializeField] private Vector3 visibleScale = Vector3.one;

    [Header("After cutscene")]
    [SerializeField] private string gameplaySceneName = "Init";
    [SerializeField] private float continueDelay = 1f;
    [SerializeField] private bool rememberWatchedCutscene = true;

    private Coroutine playRoutine;
    private bool isFinishing;
    private bool advancePanel;
    private TypewriterText activeTypewriter;
    private const string CutsceneWatchedKey = "IntroComicCutsceneWatched";

    private void Awake()
    {
#if UNITY_EDITOR
        PlayerPrefs.DeleteKey(CutsceneWatchedKey);
#endif
        HideAllElements();

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipCurrentPanel);

        if (continueButton != null)
            continueButton.onClick.AddListener(ShowAllAndFinish);
    }

    private void Start()
    {
        if (rememberWatchedCutscene && PlayerPrefs.GetInt(CutsceneWatchedKey, 0) == 1)
        {
            ShowAllAndFinish();
            return;
        }

        playRoutine = StartCoroutine(PlayCutscene());
    }

    private void HideAllElements()
    {
        foreach (var panelData in panels)
        {
            if (panelData.panel == null) continue;

            panelData.panel.alpha = 0f;
            panelData.panel.transform.localScale = hiddenScale;
            panelData.panel.interactable = false;
            panelData.panel.blocksRaycasts = false;

            foreach (var entry in panelData.texts)
                entry.target?.Clear();
        }
    }

    private IEnumerator PlayCutscene()
    {
        foreach (var panelData in panels)
        {
            if (panelData.panel == null) continue;
            advancePanel = false;

            yield return ShowElement(panelData.panel);

            foreach (var entry in panelData.texts)
            {
                if (entry.target == null || string.IsNullOrEmpty(entry.text))
                {
#if UNITY_EDITOR
                    if (entry.target == null)
                        Debug.LogWarning($"[ComicCutscene] Panel '{panelData.panel.name}' has a text entry ({entry.type}) with no TypewriterText assigned.");
#endif
                    continue;
                }

                if (!advancePanel && entry.delayBefore > 0f)
                    yield return WaitOrAdvance(entry.delayBefore);

                if (advancePanel || entry.showInstantly)
                {
                    entry.target.Play(entry.text);
                    entry.target.Skip();
                    continue;
                }

                activeTypewriter = entry.target;
                bool done = false;
                entry.target.Play(entry.text, () => done = true);
                yield return new WaitUntil(() => done || advancePanel);

                if (advancePanel && activeTypewriter != null)
                    activeTypewriter.Skip();

                activeTypewriter = null;
            }

            if (!advancePanel)
                yield return new WaitForSeconds(delayBetweenPanels);
        }

        yield return new WaitForSeconds(continueDelay);
        FinishCutscene();
    }

    private IEnumerator ShowElement(CanvasGroup element)
    {
        float timer = 0f;
        element.alpha = 0f;
        element.transform.localScale = hiddenScale;

        while (timer < fadeDuration && !advancePanel)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);
            element.alpha = smooth;
            element.transform.localScale = Vector3.Lerp(hiddenScale, visibleScale, smooth);
            yield return null;
        }

        element.alpha = 1f;
        element.transform.localScale = visibleScale;
    }

    private IEnumerator WaitOrAdvance(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && !advancePanel)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // Skip — completes current panel instantly and moves to the next one
    private void SkipCurrentPanel()
    {
        if (isFinishing) return;
        advancePanel = true;

        if (activeTypewriter != null && activeTypewriter.IsTyping)
            activeTypewriter.Skip();
    }

    // Continue — shows all panels and texts instantly, then transitions after continueDelay
    private void ShowAllAndFinish()
    {
        if (isFinishing) return;

        if (playRoutine != null)
            StopCoroutine(playRoutine);

        foreach (var panelData in panels)
        {
            if (panelData.panel == null) continue;

            panelData.panel.alpha = 1f;
            panelData.panel.transform.localScale = visibleScale;

            foreach (var entry in panelData.texts)
            {
                if (entry.target == null || string.IsNullOrEmpty(entry.text)) continue;
                entry.target.Play(entry.text);
                entry.target.Skip();
            }
        }

        StartCoroutine(DelayedFinish());
    }

    private IEnumerator DelayedFinish()
    {
        yield return new WaitForSeconds(continueDelay);
        FinishCutscene();
    }

    private void FinishCutscene()
    {
        if (isFinishing) return;
        isFinishing = true;

        if (rememberWatchedCutscene)
        {
            PlayerPrefs.SetInt(CutsceneWatchedKey, 1);
            PlayerPrefs.Save();
        }

        if (!string.IsNullOrEmpty(gameplaySceneName))
            SceneManager.LoadScene(gameplaySceneName);
    }
}
