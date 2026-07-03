using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private float charsPerSecond = 30f;

    private Coroutine typingCoroutine;
    private string fullText;
    private Action onComplete;

    private void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();
    }

    public void Play(string text, Action onComplete = null)
    {
        this.fullText = text;
        this.onComplete = onComplete;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        textComponent.text = string.Empty;
        gameObject.SetActive(true);
        typingCoroutine = StartCoroutine(TypeRoutine());
    }

    public void Skip()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (textComponent != null)
            textComponent.text = fullText;

        onComplete?.Invoke();
        onComplete = null;
    }

    public void Clear()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (textComponent != null)
            textComponent.text = string.Empty;

        gameObject.SetActive(false);
    }

    public bool IsTyping => typingCoroutine != null;

    private IEnumerator TypeRoutine()
    {
        float delay = charsPerSecond > 0 ? 1f / charsPerSecond : 0f;

        for (int i = 0; i <= fullText.Length; i++)
        {
            textComponent.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(delay);
        }

        typingCoroutine = null;
        onComplete?.Invoke();
        onComplete = null;
    }
}
