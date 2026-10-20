using System;
using System.Collections.Generic;
using MonsterWheel.Core;
using NUnit.Framework;

namespace MonsterWheel.Tests
{
    public class WheelStateMachineTests
    {
        [Test]
        public void FullCycle()
        {
            var sm = new WheelStateMachine();
            var seen = new List<WheelState>();
            sm.StateChanged += (_, to) => seen.Add(to);

            Assert.IsTrue(sm.Press());
            Assert.IsTrue(sm.Press());
            sm.NotifyReelStopped();
            sm.NotifyRingStopped();

            CollectionAssert.AreEqual(
                new[] { WheelState.Spinning, WheelState.StoppingReel, WheelState.StoppingRing, WheelState.Idle },
                seen);
            Assert.AreEqual(WheelState.Idle, sm.State);
        }

        [Test]
        public void Press_IgnoredWhileStopping()
        {
            var sm = new WheelStateMachine();
            sm.Press();
            sm.Press();
            Assert.IsFalse(sm.Press());
            Assert.AreEqual(WheelState.StoppingReel, sm.State);

            sm.NotifyReelStopped();
            Assert.IsFalse(sm.Press());
            Assert.AreEqual(WheelState.StoppingRing, sm.State);
        }

        [Test]
        public void NotifyReelStopped_InWrongState_Throws()
        {
            var sm = new WheelStateMachine();
            Assert.Throws<InvalidOperationException>(sm.NotifyReelStopped);
            Assert.Throws<InvalidOperationException>(sm.NotifyRingStopped);
        }
    }
}
