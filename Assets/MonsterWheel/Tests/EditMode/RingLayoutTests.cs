using System.Linq;
using MonsterWheel.Core;
using NUnit.Framework;

namespace MonsterWheel.Tests
{
    public class RingLayoutTests
    {
        [Test]
        public void BuildIconOrder_Has18Slots()
        {
            Assert.AreEqual(18, RingLayout.BuildIconOrder().Length);
        }

        [Test]
        public void EachIcon_AppearsTwice()
        {
            var order = RingLayout.BuildIconOrder();
            for (var icon = 0; icon < RingLayout.IconCount; icon++)
                Assert.AreEqual(2, order.Count(x => x == icon), $"icon {icon}");
        }

        [Test]
        public void IsShiftedTwoSlotsClockwise()
        {
            var order = RingLayout.BuildIconOrder();
            Assert.AreEqual(7, order[0]);
            Assert.AreEqual(8, order[1]);
            Assert.AreEqual(0, order[2]);
            Assert.AreEqual(0, order[11]);
        }

        [Test]
        public void IsPointSymmetric()
        {
            var order = RingLayout.BuildIconOrder();
            for (var i = 0; i < 9; i++)
                Assert.AreEqual(order[i], order[i + 9], $"slot {i}");
        }
    }
}
