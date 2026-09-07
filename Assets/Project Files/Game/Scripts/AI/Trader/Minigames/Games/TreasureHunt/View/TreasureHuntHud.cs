using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class TreasureHuntHud : MonoBehaviour
    {
        [SerializeField] TMP_Text hintText;
        [SerializeField] TMP_Text digsText;

        [BoxGroup("Captions", "Captions")]
        [SerializeField] string digsFormat = "Digs left: {0}";
        [BoxGroup("Captions")]
        [SerializeField] string startCaption = "Dig for the treasure";
        [BoxGroup("Captions")]
        [SerializeField] string warmerCaption = "Warmer";
        [BoxGroup("Captions")]
        [SerializeField] string colderCaption = "Colder";
        [BoxGroup("Captions")]
        [SerializeField] string foundCaption = "You found it!";
        [BoxGroup("Captions")]
        [SerializeField] string outOfDigsCaption = "Out of digs!";

        [BoxGroup("Colors", "Colors")]
        [SerializeField] Color neutralColor = new(0.196f, 0.196f, 0.196f, 1f);
        [BoxGroup("Colors")]
        [SerializeField] Color foundColor = new(0.16f, 0.6f, 0.24f, 1f);
        [BoxGroup("Colors")]
        [SerializeField] Color lostColor = new(0.75f, 0.2f, 0.18f, 1f);

        [BoxGroup("Feedback", "Feedback")]
        [SerializeField, Min(1f)] float punchScale = 1.15f;
        [BoxGroup("Feedback")]
        [SerializeField, Min(0.01f)] float punchDuration = 0.12f;

        private TweenCase hintPunchCase;
        private TweenCase digsPunchCase;

        public void Setup(int digsLeft)
        {
            SetDigsLeft(digsLeft);
            ShowStart();
        }

        public void SetDigsLeft(int digsLeft)
        {
            if (digsText == null)
                return;

            digsText.text = string.Format(digsFormat, Mathf.Max(0, digsLeft));

            digsPunchCase = Punch(digsPunchCase, digsText.transform);
        }

        public void ShowStart() => SetMessage(startCaption, neutralColor, false);

        public void ShowFound() => SetMessage(foundCaption, foundColor);

        public void ShowOutOfDigs() => SetMessage(outOfDigsCaption, lostColor);

        public void ShowHint(TreasureHint hint, TreasureHintBand band)
        {
            var color = band != null ? band.Color : neutralColor;

            switch (hint.Kind)
            {
                case TreasureHintKind.Found:
                    ShowFound();
                    break;

                case TreasureHintKind.Warmer:
                    SetMessage(warmerCaption, color);
                    break;

                case TreasureHintKind.Colder:
                    SetMessage(colderCaption, color);
                    break;

                default:
                    SetMessage(band != null ? band.Caption : string.Empty, color);
                    break;
            }
        }

        public void SetMessage(string message, Color color, bool punch = true)
        {
            if (hintText == null)
                return;

            hintText.text = message;
            hintText.color = color;

            if (punch)
                hintPunchCase = Punch(hintPunchCase, hintText.transform);
        }

        private TweenCase Punch(TweenCase activeCase, Transform target)
        {
            activeCase.KillActive();

            target.localScale = Vector3.one;

            return target.DOPushScale(punchScale, 1f, punchDuration, punchDuration, Ease.Type.SineOut, Ease.Type.SineIn);
        }

        private void OnDisable()
        {
            hintPunchCase.KillActive();
            digsPunchCase.KillActive();
        }
    }
}
