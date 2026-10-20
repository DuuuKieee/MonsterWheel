using System;
using MonsterWheel.Core;
using NUnit.Framework;

namespace MonsterWheel.Tests
{
    public class DecelerationMotionTests
    {
        [Test]
        public void Easing_Endpoints()
        {
            Assert.AreEqual(0f, Easing.OutCubic(0f), 1e-5f);
            Assert.AreEqual(1f, Easing.OutCubic(1f), 1e-5f);
            Assert.AreEqual(0f, Easing.OutBack(0f, 1.2f), 1e-5f);
            Assert.AreEqual(1f, Easing.OutBack(1f, 1.2f), 1e-5f);
        }

        [Test]
        public void Motion_EndsExactlyAtEnd_FixedDt()
        {
            var m = new DecelerationMotion(3.4f, 40f, 2f, EaseKind.OutCubic);
            for (var i = 0; i < 1000 && !m.IsDone; i++) m.Advance(1f / 60f);
            Assert.IsTrue(m.IsDone);
            Assert.AreEqual(40f, m.Position);
        }

        [Test]
        public void Motion_EndsExactlyAtEnd_RandomDt()
        {
            var rng = new Random(42);
            var m = new DecelerationMotion(5.7f, 27f, 1.5f, EaseKind.OutBack);
            for (var i = 0; i < 10000 && !m.IsDone; i++) m.Advance(0.001f + (float)rng.NextDouble() * 0.099f);
            Assert.IsTrue(m.IsDone);
            Assert.AreEqual(m.End, m.Position);
            Assert.AreEqual(27f, m.End);
        }

        [Test]
        public void Motion_ZeroStartSpeed_IsMonotonicForOutCubic()
        {
            var m = new DecelerationMotion(0f, 20f, 2f, EaseKind.OutCubic);
            var prev = m.Position;
            while (!m.IsDone)
            {
                m.Advance(1f / 60f);
                Assert.That(m.Position, Is.GreaterThanOrEqualTo(prev));
                prev = m.Position;
            }
        }

        [Test]
        public void Motion_OvershootsThenSettles_ForOutBack()
        {
            var m = new DecelerationMotion(0f, 20f, 1.5f, EaseKind.OutBack, 1.2f);
            var max = m.Position;
            while (!m.IsDone)
            {
                m.Advance(1f / 60f);
                max = Math.Max(max, m.Position);
            }
            Assert.That(max, Is.GreaterThan(20f));
            Assert.AreEqual(20f, m.Position);
        }
    }
}
