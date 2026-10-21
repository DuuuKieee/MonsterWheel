using System;
using MonsterWheel.Core;
using NUnit.Framework;

namespace MonsterWheel.Tests
{
    public class WindowFitTests
    {
        [TestCase(2560, 1400)]
        [TestCase(720, 1280)]
        public void ScreenBigEnough_KeepsTargetSize(int availableWidth, int availableHeight)
        {
            Assert.AreEqual((720, 1280), WindowFit.Fit(availableWidth, availableHeight, 720, 1280));
        }

        [Test]
        public void ShortScreen_ShrinksToHeight_Keeping9By16()
        {
            Assert.AreEqual((562, 1000), WindowFit.Fit(1920, 1000, 720, 1280));
        }

        [Test]
        public void NarrowScreen_ShrinksToWidth_Keeping9By16()
        {
            Assert.AreEqual((500, 888), WindowFit.Fit(500, 2000, 720, 1280));
        }

        [Test]
        public void Result_AlwaysFits_AndKeepsAspect()
        {
            var rng = new Random(1);
            for (var i = 0; i < 1000; i++)
            {
                var aw = rng.Next(200, 4000);
                var ah = rng.Next(200, 4000);
                var (w, h) = WindowFit.Fit(aw, ah, 720, 1280);
                Assert.That(w, Is.LessThanOrEqualTo(Math.Min(aw, 720)), $"{aw}x{ah}");
                Assert.That(h, Is.LessThanOrEqualTo(Math.Min(ah, 1280)), $"{aw}x{ah}");
                Assert.AreEqual(720.0 / 1280.0, (double)w / h, 0.01, $"{aw}x{ah} -> {w}x{h}");
            }
        }
    }
}
