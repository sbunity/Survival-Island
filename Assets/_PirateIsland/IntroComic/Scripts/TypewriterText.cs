using TMPro;
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(TMP_Text))]
    public class TypewriterText : MonoBehaviour
    {
        [SerializeField] float charactersPerSecond = 40f;

        private TMP_Text textComponent;
        private TMP_Text Text => textComponent != null ? textComponent : textComponent = GetComponent<TMP_Text>();

        private TweenCase typingTweenCase;
        private SimpleCallback completedCallback;

        public int CharacterCount
        {
            get
            {
                Text.ForceMeshUpdate();

                return Text.textInfo.characterCount;
            }
        }

        public void Clear()
        {
            typingTweenCase.KillActive();
            completedCallback = null;

            Text.maxVisibleCharacters = 0;
        }

        public void Play(SimpleCallback onCompleted)
        {
            typingTweenCase.KillActive();
            completedCallback = onCompleted;

            int characterCount = CharacterCount;
            if (characterCount == 0 || charactersPerSecond <= 0f)
            {
                Complete();

                return;
            }

            Text.maxVisibleCharacters = 0;
            typingTweenCase = Tween.DoFloat(0f, characterCount, characterCount / charactersPerSecond, value =>
            {
                Text.maxVisibleCharacters = Mathf.CeilToInt(value);
            }).OnComplete(Complete);
        }

        public void Complete()
        {
            typingTweenCase.KillActive();
            Text.maxVisibleCharacters = int.MaxValue;

            SimpleCallback callback = completedCallback;
            completedCallback = null;
            callback?.Invoke();
        }

        private void OnDestroy()
        {
            typingTweenCase.KillActive();
        }
    }
}
