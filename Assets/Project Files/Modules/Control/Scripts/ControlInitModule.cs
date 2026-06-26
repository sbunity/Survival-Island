using System.Collections;
using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Control Manager")]
    public class ControlInitModule : InitModule
    {
        public override string ModuleName => "Control Manager";

        [SerializeField] bool selectAutomatically = true;

        [HideIf("selectAutomatically")]
        [SerializeField] InputType inputType;

        [HideIf("IsJoystickCondition")]
        [SerializeField] GamepadData gamepadData;

        public override IEnumerator InitAsync(GameObject owner)
        {
            if (selectAutomatically)
                inputType = ControlUtils.GetCurrentInputType();

            Control.Init(inputType, gamepadData, owner);

            if(inputType == InputType.Keyboard)
            {
                KeyboardControl keyboardControl = owner.AddComponent<KeyboardControl>();
                keyboardControl.Init();
            }
            else if(inputType == InputType.Gamepad)
            {
                GamepadControl gamepadControl = owner.AddComponent<GamepadControl>();
                gamepadControl.Init();
            }

            yield break;
        }

        private bool IsJoystickCondition()
        {
            return selectAutomatically ? ControlUtils.GetCurrentInputType() == InputType.UIJoystick : inputType == InputType.UIJoystick;
        }
    }
}
