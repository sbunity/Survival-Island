using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class TraderPresence
    {
        private static readonly List<WanderingTraderBehavior> tradersAtBase = new();

        public static bool IsTraderAtBase => tradersAtBase.Count > 0;

        public static WanderingTraderBehavior TraderAtBase => tradersAtBase.Count > 0 ? tradersAtBase[0] : null;

        public static event SimpleCallback Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            tradersAtBase.Clear();

            Changed = null;
        }

        public static void SetAtBase(WanderingTraderBehavior trader, bool isAtBase)
        {
            if (trader == null)
                return;

            var index = tradersAtBase.IndexOf(trader);

            if (isAtBase == (index >= 0))
                return;

            if (isAtBase)
                tradersAtBase.Add(trader);
            else
                tradersAtBase.RemoveAt(index);

            Changed?.Invoke();
        }
    }
}
