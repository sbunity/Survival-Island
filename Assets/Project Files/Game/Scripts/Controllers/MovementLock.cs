namespace Watermelon
{
    public static class MovementLock
    {
        private static int counter;

        public static bool IsLocked => counter > 0;

        public static void Acquire()
        {
            counter++;

            if (counter != 1)
                return;

            Control.DisableMovementControl();

            SetJoystickVisible(false);
        }

        public static void Release()
        {
            if (counter == 0)
                return;

            counter--;

            if (counter != 0)
                return;

            Control.EnableMovementControl();

            SetJoystickVisible(true);
        }

        public static void ReleaseAll()
        {
            if (counter == 0)
                return;

            counter = 0;

            Control.EnableMovementControl();

            SetJoystickVisible(true);
        }

        private static void SetJoystickVisible(bool isVisible)
        {
            if (!UIController.HasPage<UIGame>())
                return;

            var joystick = UIController.GetPage<UIGame>().Joystick;
            if (joystick == null)
                return;

            if (isVisible)
                joystick.ShowVisuals();
            else
                joystick.HideVisuals();
        }
    }
}
