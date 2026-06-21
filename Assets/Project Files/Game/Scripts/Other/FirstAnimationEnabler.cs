using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class FirstAnimationEnabler : MonoBehaviour
    {
        [SerializeField, UniqueID] string id;

        public void Start()
        {
            WorldData worldData = WorldController.CurrentWorld;
            SaveFile worldSave = SaveController.GetFile(worldData.ID);

            SimpleBoolSave alreadyPlayedAnimationSave = worldSave.GetSaveObject<SimpleBoolSave>("FirstAnimation" + id);

            if (!alreadyPlayedAnimationSave.Value)
            {
                Control.DisableMovementControl();
                PlayerBehavior.GetBehavior().PlayerGraphics.RunWakeUpAnimation();

                alreadyPlayedAnimationSave.Value = true;
                
                worldSave.MarkAsDirty();

                Tween.DelayedCall(3f, () =>
                {
                    Control.EnableMovementControl();
                });
            }
        }
    }
}
