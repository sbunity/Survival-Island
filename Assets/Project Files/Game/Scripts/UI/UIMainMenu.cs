using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    public class UIMainMenu : UIPage
    {
        [SerializeField] RectTransform safeAreaRectTransform;

        [Space]
        [SerializeField] Button playButton;
        [SerializeField] Button newGameButton;
        [SerializeField] Button quitButton;

        [Space]
        [SerializeField] GameObject newGameConfirmationPopUp;
        [SerializeField] Button newGameConfirmedButton;
        [SerializeField] Button newGameCancelButton;

        public override void Init()
        {
            SafeAreaAdapter.RegisterRectTransform(safeAreaRectTransform);

            playButton.onClick.AddListener(OnPlayButtonClicked);
            newGameButton.onClick.AddListener(OnNewGameButtonClicked);

            newGameConfirmedButton.onClick.AddListener(OnNewGameConfirmedButtonClicked);
            newGameCancelButton.onClick.AddListener(OnNewGameCanceledButtonClicked);


#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Destroy(quitButton.gameObject);
#else 
            quitButton.onClick.AddListener(OnQuitButtonClicked);
#endif
        }

        #region Show/Hide

        protected override void OnShow()
        {
            NotifyOpened();
        }

        protected override void OnHide()
        {
            NotifyClosed();
        }

        #endregion

        #region Buttons

        public void OnPlayButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.GetClip("button_sound"));

            Overlay.Show(0.3f, () =>
            {
                SceneManager.LoadScene("Game");
            });
        }

        public void OnNewGameButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.GetClip("button_sound"));

            newGameConfirmationPopUp.SetActive(true);
        }

        public void OnNewGameConfirmedButtonClicked()
        {
            newGameConfirmationPopUp.SetActive(false);

            WorldsDatabase worldsDatabase = WorldController.Database;
            foreach(WorldData worldData in worldsDatabase.Worlds)
            {
                SaveController.DeleteFile(worldData.ID);
            }

            Overlay.Show(0.3f, () =>
            {
                SceneManager.LoadScene("Game");

                Overlay.Hide(0.3f, null);
            });

            OnPlayButtonClicked();
        }

        public void OnNewGameCanceledButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.GetClip("button_sound"));

            newGameConfirmationPopUp.SetActive(false);
        }

        public void OnQuitButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.GetClip("button_sound"));

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion
    }


}
