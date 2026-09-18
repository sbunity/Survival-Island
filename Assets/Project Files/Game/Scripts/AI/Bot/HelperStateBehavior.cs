using UnityEngine;

namespace Watermelon
{
    public abstract class HelperStateBehavior : StateBehavior<HelperBehavior>
    {
        protected NavMeshAgentBehaviour navMeshAgent;

        protected virtual float MaxDuration => 0f;

        private float watchdogDeadline = float.PositiveInfinity;

        public HelperStateBehavior(HelperBehavior helperBehavior) : base(helperBehavior)
        {
            navMeshAgent = helperBehavior.NavMeshAgentBehaviour;
        }

        public sealed override void OnStart()
        {
            KeepAlive();

            OnEnter();
        }

        public sealed override void OnUpdate()
        {
            if (Time.time >= watchdogDeadline)
            {
                watchdogDeadline = float.PositiveInfinity;

                OnWatchdogTimeout();

                return;
            }

            OnTick();
        }

        public sealed override void OnEnd()
        {
            watchdogDeadline = float.PositiveInfinity;

            OnExit();
        }

        protected virtual void OnEnter() { }

        protected virtual void OnTick() { }

        protected virtual void OnExit() { }

        protected void KeepAlive()
        {
            watchdogDeadline = MaxDuration > 0f ? Time.time + MaxDuration : float.PositiveInfinity;
        }

        protected virtual void OnWatchdogTimeout()
        {
            target.ReportTaskUnreachable();

            InvokeOnFinished();
        }
    }
}
