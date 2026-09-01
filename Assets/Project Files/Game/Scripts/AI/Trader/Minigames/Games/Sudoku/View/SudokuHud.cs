using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class SudokuHud : MonoBehaviour
    {
        [SerializeField] RectTransform heartsRoot;
        [SerializeField] Image heartPrefab;
        [SerializeField] Sprite fullHeartSprite;
        [SerializeField] Sprite lostHeartSprite;

        [Space]
        [SerializeField] TMP_Text rulesText;
        [SerializeField] TMP_Text rewardText;

        [BoxGroup("Captions", "Captions")]
        [SerializeField] string rulesFormat = "No repeats in {0}";
        [BoxGroup("Captions")]
        [SerializeField] string rewardFormat = "Solve it all for {0}";

        [BoxGroup("Feedback", "Feedback")]
        [SerializeField, Min(1f)] float lostPunchScale = 1.4f;
        [BoxGroup("Feedback")]
        [SerializeField, Min(0.01f)] float lostPunchDuration = 0.14f;

        private readonly List<Image> hearts = new List<Image>();

        private int livesLeft;

        private TweenCase punchCase;

        public void Setup(int lives, string rules, string reward)
        {
            BuildHearts(lives);

            livesLeft = lives;

            if (rulesText != null)
                rulesText.text = string.IsNullOrEmpty(rules) ? string.Empty : string.Format(rulesFormat, rules);

            if (rewardText != null)
            {
                rewardText.text = string.IsNullOrEmpty(reward) ? string.Empty : string.Format(rewardFormat, reward);
                rewardText.gameObject.SetActive(!string.IsNullOrEmpty(reward));
            }

            RefreshHearts();
        }

        public void SetLives(int lives)
        {
            var lost = Mathf.Clamp(livesLeft - 1, 0, hearts.Count - 1);
            var hasLost = lives < livesLeft;

            livesLeft = Mathf.Clamp(lives, 0, hearts.Count);

            RefreshHearts();

            if (hasLost && hearts.Count > 0)
                Punch(hearts[lost].transform);
        }

        private void BuildHearts(int lives)
        {
            if (heartsRoot == null || heartPrefab == null)
                return;

            for (var i = 0; i < hearts.Count; i++)
            {
                if (hearts[i] != null)
                    Destroy(hearts[i].gameObject);
            }

            hearts.Clear();

            for (var i = 0; i < lives; i++)
            {
                var heart = Instantiate(heartPrefab, heartsRoot);

                heart.gameObject.SetActive(true);

                hearts.Add(heart);
            }
        }

        private void RefreshHearts()
        {
            for (var i = 0; i < hearts.Count; i++)
            {
                if (hearts[i] == null)
                    continue;

                var isAlive = i < livesLeft;

                hearts[i].sprite = isAlive ? fullHeartSprite : lostHeartSprite;
                hearts[i].enabled = hearts[i].sprite != null;
            }
        }

        private void Punch(Transform target)
        {
            punchCase.KillActive();

            target.localScale = Vector3.one;

            punchCase = target.DOPushScale(lostPunchScale, 1f, lostPunchDuration, lostPunchDuration, Ease.Type.SineOut, Ease.Type.SineIn);
        }

        private void OnDisable()
        {
            punchCase.KillActive();
        }
    }
}
