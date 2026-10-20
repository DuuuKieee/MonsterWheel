using MonsterWheel.EditorTools;
using NUnit.Framework;

namespace MonsterWheel.Tests
{
    public class SceneLayoutTests
    {
        [Test]
        public void SlotCenters_Has18Slots()
        {
            Assert.AreEqual(18, MonsterWheelSceneBuilder.SlotCenters().Length);
        }

        [TestCase(0, 1, 2, 3)]    // top row, left to right
        [TestCase(12, 11, 10, 9)] // bottom row, left to right
        public void RowColumns_AreEvenlySpaced(int a, int b, int c, int d)
        {
            var s = MonsterWheelSceneBuilder.SlotCenters();
            var gap = s[b].x - s[a].x;
            Assert.AreEqual(gap, s[c].x - s[b].x, 0.01f);
            Assert.AreEqual(gap, s[d].x - s[c].x, 0.01f);
        }
    }
}
