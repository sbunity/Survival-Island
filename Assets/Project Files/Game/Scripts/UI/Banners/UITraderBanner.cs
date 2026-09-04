namespace Watermelon
{
    public class UITraderBanner : UIHudBanner
    {
        protected override void OnInitialise()
        {
            TraderPresence.Changed += OnPresenceChanged;

            OnPresenceChanged();
        }

        protected override void OnUnload()
        {
            TraderPresence.Changed -= OnPresenceChanged;
        }

        protected override void OnClicked()
        {
            var trader = TraderPresence.TraderAtBase;
            if (trader == null)
                return;

            FocusCamera(trader.Transform.position);
        }

        private void OnPresenceChanged()
        {
            if (TraderPresence.IsTraderAtBase)
                Show();
            else
                Hide();
        }
    }
}
