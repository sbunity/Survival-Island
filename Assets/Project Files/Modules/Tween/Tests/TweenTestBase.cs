using NUnit.Framework;
using UnityEngine;

namespace Watermelon.Tests
{
    /// <summary>
    /// Base class for all Tween module tests.
    /// Creates a real Tween MonoBehaviour in [SetUp] and destroys it in [TearDown].
    /// TweenManager + TweensHolder are plain C# — manual Tick() calls drive time.
    /// </summary>
    public abstract class TweenTestBase
    {
        private GameObject tweenGO;

        // NUnit calls ALL [SetUp] methods in the hierarchy automatically,
        // so use a distinct name to avoid conflicts with subclass overrides.
        [SetUp]
        public void TweenBaseSetUp()
        {
            tweenGO = new GameObject("Tween");
            tweenGO.AddComponent<Tween>().Init(300, 30, 30);
        }

        [TearDown]
        public void TweenBaseTearDown()
        {
            if (tweenGO != null)
                Object.DestroyImmediate(tweenGO);
        }

        // ─── Time helpers ──────────────────────────────────────────────────────

        /// <summary>Advance one update tick by <paramref name="dt"/> seconds.</summary>
        protected void Tick(float dt = 0.016f, UpdateMethod method = UpdateMethod.Update)
        {
            Tween.Tweens[(int)method].Update(dt, dt);
        }

        /// <summary>Advance time in small steps until <paramref name="totalSeconds"/> elapsed.</summary>
        protected void TickFor(float totalSeconds, float step = 0.016f, UpdateMethod method = UpdateMethod.Update)
        {
            float elapsed = 0f;
            while (elapsed < totalSeconds)
            {
                float dt = Mathf.Min(step, totalSeconds - elapsed);
                Tick(dt, method);
                elapsed += dt;
            }
        }

        // ─── Factory ───────────────────────────────────────────────────────────

        internal TestTweenCase NewTween(float duration = 1f)
        {
            var t = new TestTweenCase();
            t.SetDuration(duration).StartTween();
            return t;
        }
    }

    // ─── Concrete TweenCase for testing ────────────────────────────────────────

    /// <summary>
    /// Minimal TweenCase implementation — records calls for assertion.
    /// </summary>
    internal class TestTweenCase : TweenCase
    {
        public int    InvokeCount       { get; private set; }
        public float  LastInvokeState   { get; private set; }
        public bool   DefaultCompleted  { get; private set; }
        public bool   Valid             { get; set; } = true;

        public override bool Validate() => Valid;

        public override void Invoke(float deltaTime)
        {
            InvokeCount++;
            LastInvokeState = state;
        }

        public override void DefaultComplete()
        {
            DefaultCompleted = true;
        }
    }
}
