using System.Linq;
using NUnit.Framework;

namespace Watermelon.Tests
{
    /// <summary>
    /// Tests for TweensHolder mechanics: add/remove, array reorganization,
    /// capacity growth, pause/resume, and DelayedCall/NextFrame factory helpers.
    /// </summary>
    [TestFixture]
    public class TweensHolderTests : TweenTestBase
    {
        private TweensHolder UpdateHolder => Tween.Tweens[(int)UpdateMethod.Update];

        private int ActiveCount() => UpdateHolder.Tweens.Count(t => t != null);

        // ─── Add / Remove ──────────────────────────────────────────────────────

        [Test]
        public void AddTween_IncreasesActiveTweenCount()
        {
            int before = ActiveCount();
            NewTween();
            Assert.AreEqual(before + 1, ActiveCount());
        }

        [Test]
        public void Kill_RemovesTweenFromHolder_AfterTick()
        {
            var t = NewTween();
            int before = ActiveCount();
            t.Kill();
            Tick();
            Assert.AreEqual(before - 1, ActiveCount());
        }

        [Test]
        public void Complete_RemovesTweenFromHolder_AfterCompletion()
        {
            var t = NewTween(0.1f);
            int before = ActiveCount();
            TickFor(0.2f);
            Assert.AreEqual(before - 1, ActiveCount());
        }

        // ─── Reorganization after multiple kills ───────────────────────────────

        [Test]
        public void MultipleKills_SurvivingTween_StillUpdates()
        {
            // Kill tweens at various positions; remaining tween must still advance
            var t1 = NewTween();
            var t2 = NewTween(1f);
            var t3 = NewTween();

            t1.Kill();
            t3.Kill();
            Tick(0.3f); // reorganize + advance t2

            Assert.IsTrue(t2.IsActive);
            Assert.That(t2.LastInvokeState, Is.GreaterThan(0f));
        }

        [Test]
        public void KillMiddle_ThreeRemain_AllStillUpdate()
        {
            var tweens = new TestTweenCase[5];
            for (int i = 0; i < 5; i++)
                tweens[i] = NewTween(10f);

            tweens[1].Kill();
            tweens[3].Kill();
            Tick(0.5f);

            Assert.IsTrue(tweens[0].IsActive);
            Assert.IsTrue(tweens[2].IsActive);
            Assert.IsTrue(tweens[4].IsActive);
            Assert.That(tweens[0].LastInvokeState, Is.GreaterThan(0f));
        }

        // ─── Capacity growth ───────────────────────────────────────────────────

        [Test]
        public void CapacityGrowth_BeyondInitial_NoException()
        {
            // Default init capacity is 300; add 310 to force a resize
            var extras = new TestTweenCase[310];
            for (int i = 0; i < 310; i++)
                extras[i] = NewTween(100f);

            // All should still be active
            Assert.IsTrue(extras.All(t => t.IsActive));
        }

        [Test]
        public void CapacityGrowth_ArrayDoublesOnResize()
        {
            // Force at least one resize from the initial capacity
            int initial = UpdateHolder.Tweens.Length;

            for (int i = 0; i < initial + 1; i++)
                NewTween(100f);

            Assert.AreEqual(initial * 2, UpdateHolder.Tweens.Length);
        }

        // ─── Pause / Resume ────────────────────────────────────────────────────

        [Test]
        public void PauseAll_PausesAllActiveTweens()
        {
            var t1 = NewTween();
            var t2 = NewTween();

            Tween.PauseAll();

            Assert.IsTrue(t1.IsPaused);
            Assert.IsTrue(t2.IsPaused);
        }

        [Test]
        public void ResumeAll_ResumesAllPausedTweens()
        {
            var t1 = NewTween();
            var t2 = NewTween();

            Tween.PauseAll();
            Tween.ResumeAll();

            Assert.IsFalse(t1.IsPaused);
            Assert.IsFalse(t2.IsPaused);
        }

        [Test]
        public void PauseAll_PreventsTweenAdvancement()
        {
            var t = NewTween(1f);
            Tween.PauseAll();

            int before = t.InvokeCount;
            TickFor(0.5f);
            Assert.AreEqual(before, t.InvokeCount);
        }

        // ─── DelayedCall (TweenManager factory) ───────────────────────────────

        [Test]
        public void DelayedCall_ZeroDelay_InvokesImmediately_ReturnsNull()
        {
            bool invoked = false;
            var result = Tween.DelayedCall(0f, () => invoked = true);

            Assert.IsTrue(invoked);
            Assert.IsNull(result);
        }

        [Test]
        public void DelayedCall_NegativeDelay_InvokesImmediately_ReturnsNull()
        {
            bool invoked = false;
            var result = Tween.DelayedCall(-1f, () => invoked = true);

            Assert.IsTrue(invoked);
            Assert.IsNull(result);
        }

        [Test]
        public void DelayedCall_PositiveDelay_ReturnsTweenCase()
        {
            var result = Tween.DelayedCall(1f, () => { });
            Assert.IsNotNull(result);
            result.Kill();
        }

        [Test]
        public void DelayedCall_FiresCallback_AfterDelay()
        {
            bool fired = false;
            Tween.DelayedCall(0.2f, () => fired = true);

            TickFor(0.15f);
            Assert.IsFalse(fired);

            TickFor(0.1f);
            Assert.IsTrue(fired);
        }

        // ─── NextFrame ─────────────────────────────────────────────────────────

        [Test]
        public void NextFrame_DefaultOffset_FiresAfterExpectedTicks()
        {
            // NextFrame stores framesOffset+1 internally, so with default offset=1
            // the callback fires on the 2nd Tick after creation (outside of Update loop).
            bool fired = false;
            Tween.NextFrame(() => fired = true, framesOffset: 1);

            Tick();
            Assert.IsFalse(fired, "Should not fire on first tick");

            Tick();
            Assert.IsTrue(fired, "Should fire on second tick");
        }

        [Test]
        public void NextFrame_HigherOffset_DelaysAccordingly()
        {
            bool fired = false;
            Tween.NextFrame(() => fired = true, framesOffset: 3);

            Tick(); Tick(); Tick();
            Assert.IsFalse(fired);

            Tick();
            Assert.IsTrue(fired);
        }

        // ─── RemoveAll ─────────────────────────────────────────────────────────

        [Test]
        public void RemoveAll_KillsAllActiveTweens()
        {
            var t1 = NewTween();
            var t2 = NewTween();

            Tween.RemoveAll();

            Assert.IsFalse(t1.IsActive);
            Assert.IsFalse(t2.IsActive);
        }
    }
}
