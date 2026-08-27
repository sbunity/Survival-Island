using UnityEngine;

namespace Watermelon
{
    public abstract class MinigameView : MonoBehaviour
    {
        public event MinigameFinishedCallback Finished;

        protected MinigameContext Context { get; private set; }

        public bool IsRunning { get; private set; }

        public void Run(MinigameContext context)
        {
            if (IsRunning)
                return;

            Context = context;
            IsRunning = true;

            OnRun(context);
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            IsRunning = false;

            OnStop();
        }

        protected abstract void OnRun(MinigameContext context);

        protected virtual void OnStop() { }

        protected void FinishGame(MinigameResult result)
        {
            if (!IsRunning)
                return;

            IsRunning = false;

            Finished?.Invoke(result);
        }

        protected void FinishGame(bool isWin, float score = 1f)
        {
            FinishGame(isWin ? MinigameResult.Win(score) : MinigameResult.Lose(score));
        }

        protected virtual void OnDestroy()
        {
            Finished = null;
        }
    }
}
