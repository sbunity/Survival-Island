using UnityEngine;

namespace Watermelon
{
    public static class IntroComicSave
    {
        private const string WATCHED_KEY = "IntroComicCutsceneWatched";

        public static bool IsWatched => PlayerPrefs.GetInt(WATCHED_KEY, 0) == 1;

        public static void MarkWatched()
        {
            PlayerPrefs.SetInt(WATCHED_KEY, 1);
            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            PlayerPrefs.SetInt(WATCHED_KEY, 0);
            PlayerPrefs.Save();
        }
    }
}
