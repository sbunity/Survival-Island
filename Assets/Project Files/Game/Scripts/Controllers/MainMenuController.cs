using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] UIController uiController;

        private WorldController worldController;

        private void Awake()
        {
            gameObject.CacheComponent<WorldController>(out worldController);
            
            uiController.Init();

            worldController.Initialise();

            uiController.InitPages();

            UIController.ShowPage<UIMainMenu>();
        }
    }
}