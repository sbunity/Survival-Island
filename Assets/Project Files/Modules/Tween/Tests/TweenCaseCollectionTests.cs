using NUnit.Framework;

namespace Watermelon.Tests
{
    /// <summary>
    /// Tests for TweenCaseCollection — batch tracking, OnComplete callback, Kill/Complete paths.
    ///
    /// Key scenario: collection.OnComplete must fire even when tweens are Kill()ed,
    /// not just when they Complete() naturally. This is the bug fixed via OnTerminated.
    /// </summary>
    [TestFixture]
    public class TweenCaseCollectionTests : TweenTestBase
    {
        // ─── IsComplete ────────────────────────────────────────────────────────

        [Test]
        public void IsComplete_EmptyCollection_ReturnsTrue()
        {
            var col = new TweenCaseCollection();
            Assert.IsTrue(col.IsComplete());
        }

        [Test]
        public void IsComplete_WithActiveTween_ReturnsFalse()
        {
            var col = new TweenCaseCollection();
            col.AddTween(NewTween());
            Assert.IsFalse(col.IsComplete());
        }

        [Test]
        public void IsComplete_AfterAllComplete_ReturnsTrue()
        {
            var t1 = NewTween(0.1f);
            var t2 = NewTween(0.1f);
            var col = new TweenCaseCollection();
            col.AddTween(t1);
            col.AddTween(t2);

            TickFor(0.2f);
            Assert.IsTrue(col.IsComplete());
        }

        [Test]
        public void IsComplete_AfterAllKilled_ReturnsTrue()
        {
            var t1 = NewTween();
            var t2 = NewTween();
            var col = new TweenCaseCollection();
            col.AddTween(t1);
            col.AddTween(t2);

            t1.Kill();
            t2.Kill();
            // Kill() sets IsActive=false immediately — no Tick needed
            Assert.IsTrue(col.IsComplete());
        }

        [Test]
        public void IsComplete_WhenOneStillActive_ReturnsFalse()
        {
            var t1 = NewTween();
            var t2 = NewTween();
            var col = new TweenCaseCollection();
            col.AddTween(t1);
            col.AddTween(t2);

            t1.Kill();
            Assert.IsFalse(col.IsComplete()); // t2 still running
        }

        // ─── OnComplete callback ───────────────────────────────────────────────

        [Test]
        public void OnComplete_FiresWhenAllNaturallyComplete()
        {
            bool fired = false;
            var t1 = NewTween(0.1f);
            var t2 = NewTween(0.1f);
            var col = new TweenCaseCollection();
            col.AddTween(t1);
            col.AddTween(t2);
            col.OnComplete(() => fired = true);

            TickFor(0.2f);
            Assert.IsTrue(fired);
        }

        [Test]
        public void OnComplete_NotFiredUntilAllDone()
        {
            bool fired = false;
            var t1 = NewTween(0.1f);
            var t2 = NewTween(0.5f);
            var col = new TweenCaseCollection();
            col.AddTween(t1);
            col.AddTween(t2);
            col.OnComplete(() => fired = true);

            TickFor(0.15f); // only t1 done
            Assert.IsFalse(fired);

            TickFor(0.4f); // t2 done too
            Assert.IsTrue(fired);
        }

        // ─── Kill path (bug fix verification) ─────────────────────────────────

        [Test]
        public void OnComplete_FiresWhenBothKilled()
        {
            // Previously: Kill() cleared TweenCompleted delegate → callback lost.
            // Now: Kill() fires TweenTerminated → collection notified correctly.
            bool fired = false;
            var t1 = NewTween();
            var t2 = NewTween();
            var col = new TweenCaseCollection();
            col.AddTween(t1);
            col.AddTween(t2);
            col.OnComplete(() => fired = true);

            t1.Kill();
            Assert.IsFalse(fired); // t2 still active

            t2.Kill();
            Assert.IsTrue(fired); // all killed → collection done
        }

        [Test]
        public void OnComplete_FiresForMixedCompleteAndKill()
        {
            bool fired = false;
            var t1 = NewTween(0.1f); // will complete naturally
            var t2 = NewTween();     // will be killed
            var col = new TweenCaseCollection();
            col.AddTween(t1);
            col.AddTween(t2);
            col.OnComplete(() => fired = true);

            TickFor(0.2f);           // t1 completes, t2 still running
            Assert.IsFalse(fired);

            t2.Kill();               // kills t2 → all done
            Assert.IsTrue(fired);
        }

        [Test]
        public void OnComplete_NotFiredWhenOnlyOneKilled_OtherStillRunning()
        {
            bool fired = false;
            var t1 = NewTween();
            var t2 = NewTween();
            var col = new TweenCaseCollection();
            col.AddTween(t1);
            col.AddTween(t2);
            col.OnComplete(() => fired = true);

            t1.Kill();
            Assert.IsFalse(fired);
        }

        // ─── Batch Kill / Complete ─────────────────────────────────────────────

        [Test]
        public void Kill_StopsAllTweens()
        {
            var t1 = NewTween();
            var t2 = NewTween();
            var col = new TweenCaseCollection();
            col.AddTween(t1);
            col.AddTween(t2);

            col.Kill();
            Assert.IsFalse(t1.IsActive);
            Assert.IsFalse(t2.IsActive);
        }

        [Test]
        public void Complete_CompletesAllTweens()
        {
            var t1 = NewTween();
            var t2 = NewTween();
            var col = new TweenCaseCollection();
            col.AddTween(t1);
            col.AddTween(t2);

            col.Complete();
            Assert.IsTrue(t1.IsCompleted);
            Assert.IsTrue(t2.IsCompleted);
        }

        [Test]
        public void Kill_TriggersOnCompleteCallback()
        {
            bool fired = false;
            var t1 = NewTween();
            var col = new TweenCaseCollection();
            col.AddTween(t1);
            col.OnComplete(() => fired = true);

            col.Kill();
            Assert.IsTrue(fired);
        }

        [Test]
        public void Complete_TriggersOnCompleteCallback()
        {
            bool fired = false;
            var t1 = NewTween();
            var col = new TweenCaseCollection();
            col.AddTween(t1);
            col.OnComplete(() => fired = true);

            col.Complete();
            Assert.IsTrue(fired);
        }

        // ─── Operator + ────────────────────────────────────────────────────────

        [Test]
        public void OperatorPlus_NullCollection_CreatesNewAndAdds()
        {
            var tween = NewTween();
            TweenCaseCollection col = null;
            col += tween;

            Assert.IsNotNull(col);
            Assert.IsFalse(col.IsComplete());
        }

        [Test]
        public void OperatorPlus_ExistingCollection_Appends()
        {
            var t1 = NewTween();
            var t2 = NewTween();
            var col = new TweenCaseCollection();
            col.AddTween(t1);
            col = col + t2;

            Assert.AreEqual(2, col.TweenCases.Count);
        }
    }
}
