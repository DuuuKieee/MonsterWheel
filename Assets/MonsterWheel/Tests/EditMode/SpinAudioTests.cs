using MonsterWheel.View;
using NUnit.Framework;
using UnityEngine;

namespace MonsterWheel.Tests
{
    public class SpinAudioTests
    {
        [Test]
        public void Slice_CopiesTheRequestedRange()
        {
            const int freq = 1000;
            var source = AudioClip.Create("src", 2 * freq, 2, freq, false);
            var data = new float[2 * freq * 2];
            for (var i = 0; i < data.Length; i++) data[i] = i / (float)data.Length;
            source.SetData(data, 0);

            var slice = SpinAudio.Slice(source, 0.5f, 1.25f, "slice");

            Assert.AreEqual(750, slice.samples);
            Assert.AreEqual(2, slice.channels);
            Assert.AreEqual(freq, slice.frequency);
            var got = new float[slice.samples * 2];
            slice.GetData(got, 0);
            Assert.AreEqual(data[500 * 2], got[0], 1e-6f);
            Assert.AreEqual(data[(500 + 749) * 2 + 1], got[got.Length - 1], 1e-6f);

            Object.DestroyImmediate(source);
            Object.DestroyImmediate(slice);
        }

        [Test]
        public void Slice_ClampsEndToClipLength()
        {
            var source = AudioClip.Create("src", 1000, 1, 1000, false);
            var slice = SpinAudio.Slice(source, 0.6f, 5f, "tail");
            Assert.AreEqual(400, slice.samples);
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(slice);
        }
    }
}
