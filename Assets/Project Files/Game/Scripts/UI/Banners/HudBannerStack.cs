using UnityEngine;

namespace Watermelon
{
    public class HudBannerStack : MonoBehaviour
    {
        [SerializeField, Min(0f)] float topOffset = 352f;
        [SerializeField, Min(0f)] float spacing = 12f;

        [Space]
        [SerializeField] UIHudBanner[] banners;

        private UIHudBanner[] orderedBanners;

        private bool isInitialised;

        public void Initialise()
        {
            if (isInitialised)
                return;

            orderedBanners = BuildOrderedBanners();

            isInitialised = true;

            for (var i = 0; i < orderedBanners.Length; i++)
            {
                orderedBanners[i].VisibilityChanged += OnBannerVisibilityChanged;
                orderedBanners[i].Initialise();
            }
        }

        public void Unload()
        {
            if (!isInitialised)
                return;

            isInitialised = false;

            for (var i = 0; i < orderedBanners.Length; i++)
            {
                if (orderedBanners[i] == null)
                    continue;

                orderedBanners[i].VisibilityChanged -= OnBannerVisibilityChanged;
                orderedBanners[i].Unload();
            }
        }

        private void OnDestroy()
        {
            Unload();
        }

        private void OnBannerVisibilityChanged()
        {
            Relayout(true);
        }

        private void Relayout(bool animated)
        {
            var offset = topOffset;

            for (var i = 0; i < orderedBanners.Length; i++)
            {
                var banner = orderedBanners[i];
                if (banner == null)
                    continue;

                banner.ApplySlot(-offset, animated);

                if (banner.IsShown)
                    offset += banner.Height + spacing;
            }
        }

        private UIHudBanner[] BuildOrderedBanners()
        {
            var count = 0;

            for (var i = 0; banners != null && i < banners.Length; i++)
                if (banners[i] != null)
                    count++;

            var result = new UIHudBanner[count];
            var index = 0;

            for (var i = 0; banners != null && i < banners.Length; i++)
            {
                var banner = banners[i];
                if (banner == null)
                    continue;

                var insertAt = index;

                while (insertAt > 0 && result[insertAt - 1].Priority > banner.Priority)
                {
                    result[insertAt] = result[insertAt - 1];
                    insertAt--;
                }

                result[insertAt] = banner;
                index++;
            }

            return result;
        }
    }
}
