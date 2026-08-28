using UnityEngine;

namespace Watermelon
{
    public abstract class MinigameView : MonoBehaviour
    {
        public event MinigameFinishedCallback Finished;

        protected MinigameContext Context { get; private set; }

        public bool IsPrepared { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsFinished { get; private set; }

        public void Prepare(MinigameContext context)
        {
            if (IsPrepared)
                return;

            Context = context;
            IsPrepared = true;

            OnPrepare(context);
        }

        public void BuildIntro(MinigameIntroSequence sequence)
        {
            if (!IsPrepared || sequence == null)
                return;

            OnBuildIntro(sequence);
        }

        public void Run()
        {
            if (IsRunning || IsFinished || !IsPrepared)
                return;

            IsRunning = true;

            OnRun();
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            IsRunning = false;

            OnStop();
        }

        protected abstract void OnPrepare(MinigameContext context);

        protected virtual void OnBuildIntro(MinigameIntroSequence sequence) { }

        protected abstract void OnRun();

        protected virtual void OnStop() { }

        protected void FinishGame(MinigameResult result)
        {
            if (!IsPrepared || IsFinished)
                return;

            IsFinished = true;
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
