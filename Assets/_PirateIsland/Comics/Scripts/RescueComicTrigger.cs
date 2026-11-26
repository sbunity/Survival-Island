using UnityEngine;

namespace Watermelon
{
    public class RescueComicTrigger : ComicTrigger
    {
        [SerializeField] HelperBehavior helper;

        protected override bool CanArm() => helper != null && !helper.IsOpened;

        protected override void Subscribe() => helper.HelperUnlocked += Fire;

        protected override void Unsubscribe() => helper.HelperUnlocked -= Fire;
    }
}
