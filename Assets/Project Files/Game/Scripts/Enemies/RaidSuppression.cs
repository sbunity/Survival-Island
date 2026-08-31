namespace Watermelon
{
    public static class RaidSuppression
    {
        private static int counter;

        public static bool IsSuppressed => counter > 0;

        public static void Acquire()
        {
            counter++;
        }

        public static void Release()
        {
            if (counter == 0)
                return;

            counter--;
        }

        public static void ReleaseAll()
        {
            counter = 0;
        }
    }
}
