using System;
using MonsterWheel.Core;
using NUnit.Framework;

namespace MonsterWheel.Tests
{
    public class StopPlannerTests
    {
        private int[] _layout;

        [SetUp]
        public void SetUp() => _layout = RingLayout.BuildIconOrder();

        [Test]
        public void Result_LandsOnTargetIcon()
        {
            for (var c = 0; c < 18; c++)
            for (var t = 0; t < 9; t++)
            {
                var n = StopPlanner.StepsToTarget(c, t, _layout);
                Assert.AreEqual(t, _layout[(c + n) % 18], $"slot {c} icon {t}");
            }
        }

        [Test]
        public void Result_IsWithinOneToTwoLaps()
        {
            for (var c = 0; c < 18; c++)
            for (var t = 0; t < 9; t++)
            {
                var n = StopPlanner.StepsToTarget(c, t, _layout);
                Assert.That(n, Is.GreaterThanOrEqualTo(18).And.LessThan(36), $"slot {c} icon {t}");
            }
        }

        [Test]
        public void Result_IsFirstClockwiseMatch()
        {
            for (var c = 0; c < 18; c++)
            for (var t = 0; t < 9; t++)
            {
                var n = StopPlanner.StepsToTarget(c, t, _layout);
                for (var m = 18; m < n; m++)
                    Assert.AreNotEqual(t, _layout[(c + m) % 18], $"earlier match at {m} for slot {c} icon {t}");
            }
        }

        [Test]
        public void WrapAround_FromSlot17()
        {
            Assert.AreEqual(19, StopPlanner.StepsToTarget(17, 0, _layout));
        }

        [Test]
        public void SameSlotIcon_RequiresFullLap()
        {
            Assert.AreEqual(18, StopPlanner.StepsToTarget(0, 0, _layout));
        }

        [Test]
        public void MissingIcon_Throws()
        {
            Assert.Throws<ArgumentException>(() => StopPlanner.StepsToTarget(0, 99, _layout));
        }

        [Test]
        public void SlotOutOfRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => StopPlanner.StepsToTarget(18, 0, _layout));
        }
    }
}
