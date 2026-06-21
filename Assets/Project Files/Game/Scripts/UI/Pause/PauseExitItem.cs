using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class PauseExitItem : PauseItem
    {
        protected override void Awake()
        {
            base.Awake();

            if (!SceneUtils.DoesSceneExist("Menu"))
            {
                Destroy(gameObject);
            }
        }

        protected override void Click()
        {
            Time.timeScale = 0.1f;

            UIController.HidePage<UIPause>(() => 
            {
                GameController.OpenMainMenu();
            });
        }
    }
}
