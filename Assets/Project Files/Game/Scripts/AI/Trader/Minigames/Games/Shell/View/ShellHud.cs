using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class ShellHud : MonoBehaviour
    {
        [SerializeField] TMP_Text phaseText;

        [BoxGroup("Captions", "Captions")]
        [SerializeField] string watchCaption = "Remember where the prize goes";
        [BoxGroup("Captions")]
        [SerializeField] string shuffleCaption = "Keep your eyes on the shell";
        [BoxGroup("Captions")]
        [SerializeField] string pickCaption = "Pick a shell";
        [BoxGroup("Captions")]
        [SerializeField] string wonCaption = "Found it!";
        [BoxGroup("Captions")]
        [SerializeField] string lostCaption = "Empty";

        [BoxGroup("Feedback", "Feedback")]
        [SerializeField, Min(1f)] float phasePunchScale = 1.15f;
        [BoxGroup("Feedback")]
        [SerializeField, Min(0.01f)] float phasePunchDuration = 0.12f;

        private TweenCase punchCase;

        public void SetPhase(ShellPhase phase)
        {
            if (phaseText == null)
                return;

            phaseText.text = GetCaption(phase);

            Punch(phaseText.transform);
        }

        private string GetCaption(ShellPhase phase)
        {
            return phase switch
            {
                ShellPhase.Watch => watchCaption,
                ShellPhase.Shuffle => shuffleCaption,
                ShellPhase.Pick => pickCaption,
                ShellPhase.Won => wonCaption,
                _ => lostCaption,
            };
        }

        private void Punch(Transform target)
        {
            punchCase.KillActive();

            target.localScale = Vector3.one;

            punchCase = target.DOPushScale(phasePunchScale, 1f, phasePunchDuration, phasePunchDuration, Ease.Type.SineOut, Ease.Type.SineIn);
        }

        private void OnDisable()
        {
            punchCase.KillActive();
        }
    }
}
