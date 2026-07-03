using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ComicCutsceneController : MonoBehaviour
{
    [Header("Comic panels in display order")]
    [SerializeField] private List<CanvasGroup> comicElements = new List<CanvasGroup>();

    [Header("Panel texts (optional, same order as panels)")]
    [SerializeField, TextArea(2, 4)] private List<string> panelTexts = new List<string>();

    [Header("Controls")]
    [SerializeField] private Button skipButton;
    [SerializeField] private Button continueButton;

    [Header("Animation")]
    [SerializeField] private float delayBetweenElements = 0.25f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.96f, 0.96f, 1f);
    [SerializeField] private Vector3 visibleScale = Vector3.one;

    [Header("After cutscene")]
    [SerializeField] private string gameplaySceneName = "First Island";
    [SerializeField] private bool rememberWatchedCutscene = true;

    private Coroutine playRoutine;
    private bool isFinishing;
    private TypewriterText activeTypewriter;
    private const string CutsceneWatchedKey = "IntroComicCutsceneWatched";

    private void Awake()
    {
        HideAllElements();

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipCutscene);

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
            continueButton.onClick.AddListener(FinishCutscene);
        }
    }

    private void Start()
    {
        if (rememberWatchedCutscene && PlayerPrefs.GetInt(CutsceneWatchedKey, 0) == 1)
        {
            FinishCutscene();
            return;
        }

        playRoutine = StartCoroutine(PlayCutscene());
    }

    private void HideAllElements()
    {
        foreach (CanvasGroup element in comicElements)
        {
            if (element == null) continue;

            element.alpha = 0f;
            element.transform.localScale = hiddenScale;
            element.interactable = false;
            element.blocksRaycasts = false;

            var tw = element.GetComponentInChildren<TypewriterText>(true);
            tw?.Clear();
        }
    }

    private IEnumerator PlayCutscene()
    {
        for (int i = 0; i < comicElements.Count; i++)
        {
            var element = comicElements[i];
            if (element == null) continue;

            yield return ShowElement(element);

            // Start typewriter if this panel has text
            string text = (panelTexts != null && i < panelTexts.Count) ? panelTexts[i] : null;
            if (!string.IsNullOrEmpty(text))
            {
                activeTypewriter = element.GetComponentInChildren<TypewriterText>(true);
                if (activeTypewriter != null)
                {
                    bool done = false;
                    activeTypewriter.Play(text, () => done = true);
                    yield return new WaitUntil(() => done);
                }
            }

            yield return new WaitForSeconds(delayBetweenElements);
        }

        activeTypewriter = null;

        if (continueButton != null)
            continueButton.gameObject.SetActive(true);
    }

    private IEnumerator ShowElement(CanvasGroup element)
    {
        float timer = 0f;
        element.alpha = 0f;
        element.transform.localScale = hiddenScale;

        while (timer < fadeDuration)
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

    private void SkipCutscene()
    {
        if (isFinishing) return;

        // If typewriter is active — skip it first
        if (activeTypewriter != null && activeTypewriter.IsTyping)
        {
            activeTypewriter.Skip();
            return;
        }

        if (playRoutine != null)
            StopCoroutine(playRoutine);

        foreach (CanvasGroup element in comicElements)
        {
            if (element == null) continue;
            element.alpha = 1f;
            element.transform.localScale = visibleScale;

            var tw = element.GetComponentInChildren<TypewriterText>(true);
            tw?.Skip();
        }

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
