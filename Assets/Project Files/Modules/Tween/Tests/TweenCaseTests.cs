using NUnit.Framework;

namespace Watermelon.Tests
{
    /// <summary>
    /// Tests for TweenCase lifecycle, builder API, and callbacks.
    /// Uses TestTweenCase (minimal concrete subclass) and manual Tick() calls.
    /// </summary>
    [TestFixture]
    public class TweenCaseTests : TweenTestBase
    {
        private TestTweenCase _t;

        [SetUp]
        public void SetUp()
        {
            _t = new TestTweenCase();
            _t.SetDuration(1f);
        }

        // ─── IsActive / StartTween ─────────────────────────────────────────────

        [Test]
        public void BeforeStart_IsActive_IsFalse()
        {
            Assert.IsFalse(_t.IsActive);
        }

        [Test]
        public void AfterStart_IsActive_IsTrue()
        {
            _t.StartTween();
            Assert.IsTrue(_t.IsActive);
        }

        // ─── Kill ──────────────────────────────────────────────────────────────

        [Test]
        public void Kill_SetsIsActive_False_Immediately()
        {
            _t.StartTween();
            _t.Kill();
            Assert.IsFalse(_t.IsActive);
        }

        [Test]
        public void Kill_SetsIsKilling_True_Immediately()
        {
            _t.StartTween();
            _t.Kill();
            Assert.IsTrue(_t.IsKilling);
        }

        [Test]
        public void Kill_IsKilling_False_AfterTick()
        {
            // ResetKillingState() is called post-update when the tween is removed
            _t.StartTween();
            _t.Kill();
            Tick();
            Assert.IsFalse(_t.IsKilling);
        }

        [Test]
        public void Kill_IsIdempotent_SecondCallIsNoOp()
        {
            _t.StartTween();
            _t.Kill();
            _t.Kill(); // must not throw or double-add to killingTweens

            Tick();
            Assert.IsFalse(_t.IsActive);
            Assert.IsFalse(_t.IsKilling);
        }

        [Test]
        public void Kill_DoesNotFireOnComplete()
        {
            bool fired = false;
            _t.OnComplete(() => fired = true);
            _t.StartTween();
            _t.Kill();
            Assert.IsFalse(fired);
        }

        [Test]
        public void Kill_DoesNotCallDefaultComplete()
        {
            _t.StartTween();
            _t.Kill();
            Assert.IsFalse(_t.DefaultCompleted);
        }

        // ─── Complete ──────────────────────────────────────────────────────────

        [Test]
        public void Complete_SetsIsCompleted_True()
        {
            _t.StartTween();
            _t.Complete();
            Assert.IsTrue(_t.IsCompleted);
        }

        [Test]
        public void Complete_SetsIsActive_False()
        {
            _t.StartTween();
            _t.Complete();
            Assert.IsFalse(_t.IsActive);
        }

        [Test]
        public void Complete_CallsDefaultComplete()
        {
            _t.StartTween();
            _t.Complete();
            Assert.IsTrue(_t.DefaultCompleted);
        }

        [Test]
        public void Complete_FiresOnComplete_Callback()
        {
            bool fired = false;
            _t.OnComplete(() => fired = true);
            _t.StartTween();
            _t.Complete();
            Assert.IsTrue(fired);
        }

        [Test]
        public void Complete_IsIdempotent_CallbackFiresOnce()
        {
            int count = 0;
            _t.OnComplete(() => count++);
            _t.StartTween();
            _t.Complete();
            _t.Complete();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void OnComplete_SecondCall_OverwritesFirst()
        {
            // completedCallback is a plain field (= not +=), so last call wins.
            int count = 0;
            _t.OnComplete(() => count += 10)
              .OnComplete(() => count += 1); // replaces previous
            _t.StartTween();
            _t.Complete();
            Assert.AreEqual(1, count);
        }

        // ─── Natural completion via time ───────────────────────────────────────

        [Test]
        public void NaturalCompletion_SetsIsCompleted_AfterDuration()
        {
            _t.SetDuration(0.1f).StartTween();
            TickFor(0.15f);
            Assert.IsTrue(_t.IsCompleted);
        }

        [Test]
        public void NaturalCompletion_CallsDefaultComplete()
        {
            _t.SetDuration(0.1f).StartTween();
            TickFor(0.15f);
            Assert.IsTrue(_t.DefaultCompleted);
        }

        [Test]
        public void NaturalCompletion_FiresOnCompleteCallback()
        {
            bool fired = false;
            _t.SetDuration(0.1f).OnComplete(() => fired = true).StartTween();
            TickFor(0.15f);
            Assert.IsTrue(fired);
        }

        [Test]
        public void State_AdvancesProportionally_WithDeltaTime()
        {
            _t.SetDuration(1f).StartTween();
            Tick(0.5f);
            // state should be exactly 0.5 (one step of dt/duration)
            Assert.That(_t.LastInvokeState, Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void Invoke_CalledEachTick_WhileActive()
        {
            _t.SetDuration(1f).StartTween();
            Tick();
            Tick();
            Tick();
            Assert.AreEqual(3, _t.InvokeCount);
        }

        // ─── Restart ───────────────────────────────────────────────────────────

        [Test]
        public void Restart_ResetsStateToZero()
        {
            _t.SetDuration(1f).StartTween();
            Tick(0.5f);
            _t.Restart();
            Tick(0.016f);
            Assert.That(_t.LastInvokeState, Is.LessThan(0.05f));
        }

        [Test]
        public void Restart_AfterNaturalCompletion_ReActivates()
        {
            // Complete() → Kill() → ResetKillingState() all happen within TickFor()
            _t.SetDuration(0.1f).StartTween();
            TickFor(0.15f);
            Assert.IsTrue(_t.IsCompleted);

            _t.Restart();
            Assert.IsTrue(_t.IsActive);
        }

        [Test]
        public void Restart_DuringKill_IsNoOp()
        {
            // Kill() sets isKilling=true; Restart() guards against this
            _t.StartTween();
            _t.Kill();
            bool wasActive = _t.IsActive;
            _t.Restart(); // guarded by isKilling check — must not throw
            // IsActive should remain false (not re-added)
            Assert.IsFalse(_t.IsActive);
        }

        // ─── Pause / Resume ────────────────────────────────────────────────────

        [Test]
        public void Pause_SetsIsPaused_True()
        {
            _t.StartTween();
            _t.Pause();
            Assert.IsTrue(_t.IsPaused);
        }

        [Test]
        public void Resume_ClearsIsPaused()
        {
            _t.StartTween();
            _t.Pause();
            _t.Resume();
            Assert.IsFalse(_t.IsPaused);
        }

        [Test]
        public void PausedTween_DoesNotAdvance_WhenTicked()
        {
            _t.SetDuration(1f).StartTween();
            _t.Pause();
            int before = _t.InvokeCount;
            TickFor(0.5f);
            Assert.AreEqual(before, _t.InvokeCount);
        }

        // ─── Delay ─────────────────────────────────────────────────────────────

        [Test]
        public void SetDelay_TweenDoesNotInvoke_BeforeDelayElapsed()
        {
            _t.SetDuration(1f).SetDelay(0.5f).StartTween();
            TickFor(0.4f);
            Assert.AreEqual(0, _t.InvokeCount);
        }

        [Test]
        public void SetDelay_TweenInvokes_AfterDelayElapsed()
        {
            _t.SetDuration(1f).SetDelay(0.1f).StartTween();
            TickFor(0.2f);
            Assert.Greater(_t.InvokeCount, 0);
        }

        // ─── Validate ──────────────────────────────────────────────────────────

        [Test]
        public void Validate_ReturnsFalse_TweenKilledOnNextTick()
        {
            _t.StartTween();
            _t.Valid = false;
            Tick();
            Assert.IsFalse(_t.IsActive);
        }

        // ─── OnTimeReached ─────────────────────────────────────────────────────

        [Test]
        public void OnTimeReached_FiresWhenProgressPassesThreshold()
        {
            bool fired = false;
            _t.SetDuration(1f)
              .OnTimeReached(0.5f, () => fired = true)
              .StartTween();

            TickFor(0.45f);
            Assert.IsFalse(fired);

            Tick(0.1f); // crosses 0.5
            Assert.IsTrue(fired);
        }

        [Test]
        public void OnTimeReached_FiresExactlyOnce()
        {
            int count = 0;
            _t.SetDuration(1f)
              .OnTimeReached(0.3f, () => count++)
              .StartTween();

            TickFor(2f); // well past completion
            Assert.AreEqual(1, count);
        }

        [Test]
        public void OnTimeReached_Multiple_FireInOrder()
        {
            int last = 0;
            _t.SetDuration(1f)
              .OnTimeReached(0.3f, () => last = 1)
              .OnTimeReached(0.7f, () => last = 2)
              .StartTween();

            TickFor(0.5f);
            Assert.AreEqual(1, last); // only first fired

            TickFor(0.3f);
            Assert.AreEqual(2, last); // both fired
        }

        // ─── SetEasing ─────────────────────────────────────────────────────────

        [Test]
        public void SetEasing_Linear_StateMatchesInterpolation()
        {
            _t.SetDuration(1f).SetEasing(Ease.Type.Linear).StartTween();
            Tick(0.5f);
            float expected = Ease.Interpolate(0.5f, Ease.Type.Linear);
            Assert.That(_t.Interpolate(_t.LastInvokeState), Is.EqualTo(expected).Within(0.001f));
        }

        // ─── SetUnscaledMode ───────────────────────────────────────────────────

        [Test]
        public void SetUnscaledMode_True_SetsIsUnscaled()
        {
            _t.SetUnscaledMode(true);
            Assert.IsTrue(_t.IsUnscaled);
        }

        [Test]
        public void SetUnscaledMode_False_IsScaled()
        {
            _t.SetUnscaledMode(false);
            Assert.IsFalse(_t.IsUnscaled);
        }
    }
}
