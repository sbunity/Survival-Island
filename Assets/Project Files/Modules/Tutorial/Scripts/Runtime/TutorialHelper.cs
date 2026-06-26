using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public static class TutorialHelper
    {
        private const string MenuName = "Actions/Skip Tutorial";
        private const string SettingName = "IsTutorialSkipped";

        public static bool IsTutorialSkipped()
        {
#if UNITY_EDITOR
            return IsTutorialSkippedPrefs;
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        private static bool IsTutorialSkippedPrefs
        {
            get { return EditorPrefs.GetBool(SettingName, false); }
            set { EditorPrefs.SetBool(SettingName, value); }
        }

        [MenuItem(MenuName, priority = 200)]
        private static void ToggleAction()
        {
            bool tutorialState = IsTutorialSkippedPrefs;
            IsTutorialSkippedPrefs = !tutorialState;

            if (Application.isPlaying)
                TutorialController.SetTutorialSkipped(!tutorialState);
        }

        [MenuItem(MenuName, true, priority = 200)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked(MenuName, IsTutorialSkippedPrefs);

            return true;
        }
#endif
    }
}
