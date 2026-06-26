using NUnit.Framework;

namespace Watermelon.Tests
{
    /// <summary>
    /// Tests for SaveFileMetadata.IsNewerThan() — the conflict-resolution
    /// comparator used by CloudSaveManager. Pure C#, no Unity runtime needed.
    /// </summary>
    [TestFixture]
    public class SaveFileMetadataTests
    {
        // ─── Constructor ──────────────────────────────────────────────────────

        [Test]
        public void DefaultConstructor_ZeroValues()
        {
            var meta = new SaveFileMetadata();
            Assert.AreEqual(0, meta.saveCount);
            Assert.AreEqual(0, meta.timestampUnix);
        }

        [Test]
        public void ParameterizedConstructor_SetsValues()
        {
            var meta = new SaveFileMetadata(timestamp: 12345, count: 7);
            Assert.AreEqual(12345, meta.timestampUnix);
            Assert.AreEqual(7,     meta.saveCount);
        }

        // ─── IsNewerThan — null guard ─────────────────────────────────────────

        [Test]
        public void IsNewerThan_NullOther_ReturnsTrue()
        {
            var meta = new SaveFileMetadata(100, 5);
            Assert.IsTrue(meta.IsNewerThan(null));
        }

        // ─── IsNewerThan — saveCount comparison ───────────────────────────────

        [Test]
        public void IsNewerThan_HigherSaveCount_ReturnsTrue()
        {
            // Cloud has count=10 even though local has a newer timestamp
            var cloud = new SaveFileMetadata(timestamp: 100, count: 10);
            var local = new SaveFileMetadata(timestamp: 200, count:  5);

            Assert.IsTrue(cloud.IsNewerThan(local));
        }

        [Test]
        public void IsNewerThan_LowerSaveCount_ReturnsFalse()
        {
            var older = new SaveFileMetadata(timestamp: 999, count: 3);
            var newer = new SaveFileMetadata(timestamp:   1, count: 8);

            Assert.IsFalse(older.IsNewerThan(newer));
        }

        [Test]
        public void IsNewerThan_SaveCountTakesPriorityOverTimestamp()
        {
            // Cloud: higher count, much older timestamp  → should still win
            var cloud = new SaveFileMetadata(timestamp:  10, count: 20);
            var local = new SaveFileMetadata(timestamp: 999, count:  5);

            Assert.IsTrue(cloud.IsNewerThan(local));
            Assert.IsFalse(local.IsNewerThan(cloud));
        }

        // ─── IsNewerThan — timestamp tiebreak (equal saveCount) ──────────────

        [Test]
        public void IsNewerThan_EqualCount_HigherTimestamp_ReturnsTrue()
        {
            var newer = new SaveFileMetadata(timestamp: 200, count: 5);
            var older = new SaveFileMetadata(timestamp: 100, count: 5);

            Assert.IsTrue(newer.IsNewerThan(older));
        }

        [Test]
        public void IsNewerThan_EqualCount_LowerTimestamp_ReturnsFalse()
        {
            var older = new SaveFileMetadata(timestamp: 100, count: 5);
            var newer = new SaveFileMetadata(timestamp: 200, count: 5);

            Assert.IsFalse(older.IsNewerThan(newer));
        }

        [Test]
        public void IsNewerThan_EqualCountAndTimestamp_ReturnsFalse()
        {
            var a = new SaveFileMetadata(timestamp: 100, count: 5);
            var b = new SaveFileMetadata(timestamp: 100, count: 5);

            Assert.IsFalse(a.IsNewerThan(b));
        }

        // ─── IsNewerThan — zero / fresh file edge cases ───────────────────────

        [Test]
        public void IsNewerThan_ZeroCountAndTimestamp_VsZero_ReturnsFalse()
        {
            var a = new SaveFileMetadata(0, 0);
            var b = new SaveFileMetadata(0, 0);
            Assert.IsFalse(a.IsNewerThan(b));
        }

        [Test]
        public void IsNewerThan_AnyCountGreaterThanZero_NewerThanFreshFile()
        {
            var saved = new SaveFileMetadata(timestamp: 1, count: 1);
            var fresh = new SaveFileMetadata(timestamp: 0, count: 0);

            Assert.IsTrue(saved.IsNewerThan(fresh));
            Assert.IsFalse(fresh.IsNewerThan(saved));
        }

        // ─── CreateCurrent ────────────────────────────────────────────────────

        [Test]
        public void CreateCurrent_SetsSaveCount()
        {
            var meta = SaveFileMetadata.CreateCurrent(nextSaveCount: 3);
            Assert.AreEqual(3, meta.saveCount);
        }

        [Test]
        public void CreateCurrent_TimestampIsWithinCurrentSecond()
        {
            long before = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var meta    = SaveFileMetadata.CreateCurrent(nextSaveCount: 1);
            long after  = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            Assert.GreaterOrEqual(meta.timestampUnix, before);
            Assert.LessOrEqual(meta.timestampUnix,    after);
        }

        // ─── ToString ─────────────────────────────────────────────────────────

        [Test]
        public void ToString_ContainsSaveCount()
        {
            var meta = new SaveFileMetadata(0, 42);
            StringAssert.Contains("42", meta.ToString());
        }
    }
}
