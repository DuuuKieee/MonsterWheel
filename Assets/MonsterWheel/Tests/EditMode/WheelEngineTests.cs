using System;
using MonsterWheel.Core;
using NUnit.Framework;

namespace MonsterWheel.Tests
{
    public class WheelEngineTests
    {
        private static WheelEngine NewEngine(int seed) => new WheelEngine(WheelSettings.Default, new SystemRandom(seed));

        /// <summary>Press, spin ~1s, press, tick until Idle. Returns false if the round never finished.</summary>
        private static bool RunRound(WheelEngine engine, Func<float> dt, float spinSeconds = 1f)
        {
            engine.Press();
            for (var t = 0f; t < spinSeconds;)
            {
                var d = dt();
                engine.Tick(d);
                t += Math.Max(d, 0.001f);
            }
            engine.Press();
            for (var i = 0; i < 100000 && engine.State != WheelState.Idle; i++) engine.Tick(dt());
            return engine.State == WheelState.Idle;
        }

        private static void AssertMatch(WheelEngine e)
        {
            Assert.AreEqual(e.TargetIcon, e.CenterIcon, "reel icon");
            Assert.AreEqual(e.TargetIcon, e.Layout[e.ActiveSlot], "ring icon");
            Assert.IsTrue(e.HasResult);
        }

        [Test]
        public void Round_RingIconMatchesReelIcon_1000Rounds()
        {
            var engine = NewEngine(7);
            var dtRng = new Random(99);
            for (var round = 0; round < 1000; round++)
            {
                var spin = 0.2f + (float)dtRng.NextDouble() * 2f;
                Assert.IsTrue(RunRound(engine, () => 0.005f + (float)dtRng.NextDouble() * 0.045f, spin), $"round {round}");
                AssertMatch(engine);
            }
        }

        [Test]
        public void Round_ImmediateStop_Matches()
        {
            var engine = NewEngine(1);
            engine.Press();
            engine.Tick(0.001f);
            engine.Press();
            for (var i = 0; i < 100000 && engine.State != WheelState.Idle; i++) engine.Tick(1f / 60f);
            Assert.AreEqual(WheelState.Idle, engine.State);
            AssertMatch(engine);
        }

        [Test]
        public void Round_WithHugeAndZeroDt_StillMatches()
        {
            var engine = NewEngine(3);
            var flip = false;
            Assert.IsTrue(RunRound(engine, () => (flip = !flip) ? 0.5f : 0f));
            AssertMatch(engine);
        }

        [Test]
        public void Press_DuringStopSequence_IsIgnored()
        {
            var engine = NewEngine(5);
            var finished = 0;
            engine.RoundFinished += () => finished++;

            engine.Press();
            for (var i = 0; i < 60; i++) engine.Tick(1f / 60f);
            engine.Press();
            var sawRingStop = false;
            for (var i = 0; i < 100000 && engine.State != WheelState.Idle; i++)
            {
                engine.Press(); // spammed every frame, including the handoff delay
                sawRingStop |= engine.State == WheelState.StoppingRing;
                engine.Tick(1f / 60f);
            }

            Assert.IsTrue(sawRingStop);
            Assert.AreEqual(WheelState.Idle, engine.State);
            Assert.AreEqual(1, finished);
            AssertMatch(engine);
        }

        [Test]
        public void LongSpin_PositionsStayWrapped()
        {
            var engine = NewEngine(2);
            engine.Press();
            for (var i = 0; i < 30000; i++)
            {
                engine.Tick(0.02f);
                Assert.That(engine.RingPosition, Is.GreaterThanOrEqualTo(0f).And.LessThan(18f));
                Assert.That(engine.ReelPosition, Is.GreaterThanOrEqualTo(0f).And.LessThan(9f));
            }
            engine.Press();
            for (var i = 0; i < 100000 && engine.State != WheelState.Idle; i++) engine.Tick(0.02f);
            AssertMatch(engine);
        }

        [Test]
        public void Ring_MovesClockwiseOnly()
        {
            var engine = NewEngine(11);
            var prev = engine.RingPosition;
            void Check()
            {
                var d = engine.RingPosition - prev;
                d = ((d % 18f) + 18f + 9f) % 18f - 9f; // shortest signed delta on the ring
                Assert.That(d, Is.GreaterThanOrEqualTo(-1e-4f));
                prev = engine.RingPosition;
            }

            engine.Press();
            for (var i = 0; i < 60; i++) { engine.Tick(1f / 60f); Check(); }
            engine.Press();
            for (var i = 0; i < 100000 && engine.State != WheelState.Idle; i++) { engine.Tick(1f / 60f); Check(); }
            AssertMatch(engine);
        }

        [Test]
        public void SpeedRatios_AreZeroWhenIdle_AndFullWhileSpinning()
        {
            var engine = NewEngine(4);
            Assert.AreEqual(0f, engine.RingSpeedRatio);
            engine.Press();
            for (var i = 0; i < 60; i++) engine.Tick(1f / 60f);
            Assert.AreEqual(1f, engine.RingSpeedRatio, 1e-4f);
            Assert.AreEqual(1f, engine.ReelSpeedRatio, 1e-4f);
            engine.Press();
            for (var i = 0; i < 100000 && engine.State != WheelState.Idle; i++) engine.Tick(1f / 60f);
            Assert.AreEqual(0f, engine.RingSpeedRatio);
            Assert.AreEqual(0f, engine.ReelSpeedRatio);
        }
    }
}
