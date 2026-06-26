using NUnit.Framework;

namespace Watermelon.Tests
{
    /// <summary>
    /// Tests for Ease — pure math, no Tween singleton needed.
    /// </summary>
    [TestFixture]
    public class EaseTests
    {
        // ─── Linear ────────────────────────────────────────────────────────────

        [Test]
        public void Linear_AtZero_ReturnsZero()
        {
            Assert.AreEqual(0f, Ease.Interpolate(0f, Ease.Type.Linear), 1e-6f);
        }

        [Test]
        public void Linear_AtOne_ReturnsOne()
        {
            Assert.AreEqual(1f, Ease.Interpolate(1f, Ease.Type.Linear), 1e-6f);
        }

        [Test]
        public void Linear_AtHalf_ReturnsHalf()
        {
            Assert.AreEqual(0.5f, Ease.Interpolate(0.5f, Ease.Type.Linear), 1e-6f);
        }

        [Test]
        public void Linear_Arbitrary_MatchesInput()
        {
            Assert.AreEqual(0.75f, Ease.Interpolate(0.75f, Ease.Type.Linear), 1e-6f);
        }

        // ─── Boundary conditions: all standard easings return ~0 at t=0 ───────

        [TestCase(Ease.Type.Linear)]
        [TestCase(Ease.Type.QuadIn)]
        [TestCase(Ease.Type.QuadOut)]
        [TestCase(Ease.Type.CubicIn)]
        [TestCase(Ease.Type.CubicOut)]
        [TestCase(Ease.Type.CubicInOut)]
        [TestCase(Ease.Type.SineIn)]
        [TestCase(Ease.Type.SineOut)]
        [TestCase(Ease.Type.SineInOut)]
        [TestCase(Ease.Type.BackIn)]
        [TestCase(Ease.Type.BackOut)]
        [TestCase(Ease.Type.BounceIn)]
        [TestCase(Ease.Type.BounceOut)]
        [TestCase(Ease.Type.BounceInOut)]
        public void Easing_AtZero_ReturnsApproxZero(Ease.Type type)
        {
            Assert.That(Ease.Interpolate(0f, type), Is.EqualTo(0f).Within(0.01f),
                $"{type} should return ~0 at t=0");
        }

        // ─── Boundary conditions: all standard easings return ~1 at t=1 ───────

        [TestCase(Ease.Type.Linear)]
        [TestCase(Ease.Type.QuadIn)]
        [TestCase(Ease.Type.QuadOut)]
        [TestCase(Ease.Type.CubicIn)]
        [TestCase(Ease.Type.CubicOut)]
        [TestCase(Ease.Type.CubicInOut)]
        [TestCase(Ease.Type.SineIn)]
        [TestCase(Ease.Type.SineOut)]
        [TestCase(Ease.Type.SineInOut)]
        [TestCase(Ease.Type.BackIn)]
        [TestCase(Ease.Type.BackOut)]
        [TestCase(Ease.Type.BounceIn)]
        [TestCase(Ease.Type.BounceOut)]
        [TestCase(Ease.Type.BounceInOut)]
        public void Easing_AtOne_ReturnsApproxOne(Ease.Type type)
        {
            Assert.That(Ease.Interpolate(1f, type), Is.EqualTo(1f).Within(0.01f),
                $"{type} should return ~1 at t=1");
        }

        // ─── Curve shape: In (accelerating) < Linear < Out (decelerating) ─────

        [Test]
        public void QuadIn_AtHalf_LessThanLinear()
        {
            // Accelerating curve: slow start → value at midpoint < 0.5
            Assert.Less(Ease.Interpolate(0.5f, Ease.Type.QuadIn), 0.5f);
        }

        [Test]
        public void QuadOut_AtHalf_GreaterThanLinear()
        {
            // Decelerating curve: fast start → value at midpoint > 0.5
            Assert.Greater(Ease.Interpolate(0.5f, Ease.Type.QuadOut), 0.5f);
        }

        [Test]
        public void CubicIn_AtHalf_LessThanQuadIn_AtHalf()
        {
            // Higher power → stronger acceleration → even slower at midpoint
            Assert.Less(
                Ease.Interpolate(0.5f, Ease.Type.CubicIn),
                Ease.Interpolate(0.5f, Ease.Type.QuadIn));
        }

        // ─── GetFunction ───────────────────────────────────────────────────────

        [Test]
        public void GetFunction_ReturnsNonNull_ForAllBuiltInTypes()
        {
            foreach (Ease.Type type in System.Enum.GetValues(typeof(Ease.Type)))
            {
                var fn = Ease.GetFunction(type);
                Assert.IsNotNull(fn, $"GetFunction({type}) returned null");
            }
        }

        [Test]
        public void GetFunction_Linear_InterpolateMatchesStaticMethod()
        {
            var fn = Ease.GetFunction(Ease.Type.Linear);
            float p = 0.3f;
            Assert.AreEqual(
                Ease.Interpolate(p, Ease.Type.Linear),
                fn.Interpolate(p),
                1e-6f);
        }
    }
}
