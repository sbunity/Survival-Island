using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class MinigameIntroSequence
    {
        private readonly List<Step> steps = new List<Step>();
        private readonly List<TweenCase> cases = new List<TweenCase>();

        public int StepCount => steps.Count;

        public bool IsPlaying { get; private set; }

        public void Clear()
        {
            Stop();

            steps.Clear();
        }

        public void Add(MinigameIntroStage stage, SimpleCallback reveal)
        {
            if (reveal == null)
                return;

            steps.Add(new Step(stage, reveal, steps.Count));
        }

        public void Play(float stepInterval, SimpleCallback onComplete)
        {
            Stop();

            IsPlaying = true;

            steps.Sort(Step.Compare);

            if (stepInterval <= 0f)
            {
                for (var i = 0; i < steps.Count; i++)
                    steps[i].Reveal.Invoke();

                IsPlaying = false;

                onComplete?.Invoke();

                return;
            }

            for (var i = 0; i < steps.Count; i++)
            {
                var reveal = steps[i].Reveal;

                cases.Add(Tween.DelayedCall(stepInterval * i, () => reveal.Invoke()));
            }

            cases.Add(Tween.DelayedCall(stepInterval * steps.Count, () =>
            {
                IsPlaying = false;

                onComplete?.Invoke();
            }));
        }

        public void Stop()
        {
            IsPlaying = false;

            for (var i = 0; i < cases.Count; i++)
                cases[i].KillActive();

            cases.Clear();
        }

        private readonly struct Step
        {
            public readonly MinigameIntroStage Stage;
            public readonly SimpleCallback Reveal;
            public readonly int Index;

            public Step(MinigameIntroStage stage, SimpleCallback reveal, int index)
            {
                Stage = stage;
                Reveal = reveal;
                Index = index;
            }

            public static int Compare(Step first, Step second)
            {
                var byStage = first.Stage.CompareTo(second.Stage);

                return byStage != 0 ? byStage : first.Index.CompareTo(second.Index);
            }
        }
    }
}
