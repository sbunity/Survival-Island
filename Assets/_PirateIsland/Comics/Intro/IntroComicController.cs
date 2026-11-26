using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    public class IntroComicController : MonoBehaviour
    {
        [SerializeField] ComicPlayer comicPlayer;
        [SerializeField] RectTransform safeAreaRectTransform;

        [Space]
        [SerializeField] string nextSceneName = "Game";

        private void Start()
        {
            if (safeAreaRectTransform != null)
                SafeAreaAdapter.RegisterRectTransform(safeAreaRectTransform);

            Overlay.Hide(0.3f);

            comicPlayer.Play(OnComicFinished);
        }

        private void OnComicFinished()
        {
            IntroComicSave.MarkWatched();

            Overlay.Show(0.3f, () =>
            {
                SceneManager.LoadScene(nextSceneName);
            });
        }

        private void OnDestroy()
        {
            if (safeAreaRectTransform != null)
                SafeAreaAdapter.UnregisterRectTransform(safeAreaRectTransform);
        }
    }
}
