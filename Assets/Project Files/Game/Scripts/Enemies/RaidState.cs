using UnityEngine;

namespace Watermelon
{
    public static class RaidState
    {
        private static int counter;

        public static bool IsRaidActive => counter > 0;

        public static event SimpleCallback Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            counter = 0;
            Changed = null;
        }

        public static void Acquire()
        {
            counter++;

            if (counter == 1)
                Changed?.Invoke();
        }

        public static void Release()
        {
            if (counter == 0)
                return;

            counter--;

            if (counter == 0)
                Changed?.Invoke();
        }
    }
}
