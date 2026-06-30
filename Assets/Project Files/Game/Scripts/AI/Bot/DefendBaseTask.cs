using Watermelon.AI;

namespace Watermelon
{
    public class DefendBaseTask : BaseTask
    {
        public BaseAttackController Controller { get; }

        public DefendBaseTask(BaseAttackController controller)
            : base(HelperTaskType.DefendingBase, controller.DefensePoint, int.MaxValue, false)
        {
            Controller = controller;
        }

        public override bool IsTypeAvailable(HelperTaskType availableTasks) 
            => true;

        public override bool IsInRange(HelperBehavior helperBehavior) 
            => true;

        public override bool IsPathExists(HelperBehavior helperBehavior) 
            => true;

        public override bool Validate(HelperBehavior helperBehavior) 
            => IsActive &&
                Controller != null &&
                Controller.IsAlertActive &&
                helperBehavior != null &&
                helperBehavior.IsOpened &&
                !helperBehavior.IsDead &&
                !helperBehavior.IsRecovering;

        public override bool GetStateMachineState(out HelperStateMachine.State nextState)
        {
            nextState = HelperStateMachine.State.DefendingBase;
            return true;
        }

        protected override void OnTaskActivated()
        {
        }

        protected override void OnTaskDisabled()
        {
        }

        protected override void OnTaskTaken(HelperBehavior helperBehavior)
        {
        }

        protected override void OnTaskReseted()
        {
        }
    }
}
