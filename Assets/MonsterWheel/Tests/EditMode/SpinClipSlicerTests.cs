using System;
using MonsterWheel.EditorTools;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MonsterWheel.Tests
{
    public class SpinClipSlicerTests
    {
        [Test]
        public void Slice_CopiesTheRequestedRange()
        {
            const int freq = 1000;
            var source = AudioClip.Create("src", 2 * freq, 2, freq, false);
            var data = new float[2 * freq * 2];
            for (var i = 0; i < data.Length; i++) data[i] = i / (float)data.Length;
            source.SetData(data, 0);

            var slice = SpinClipSlicer.Slice(source, 0.5f, 1.25f);

            Assert.AreEqual(750 * 2, slice.Length);
            Assert.AreEqual(data[500 * 2], slice[0], 1e-6f);
            Assert.AreEqual(data[(500 + 749) * 2 + 1], slice[slice.Length - 1], 1e-6f);
            Object.DestroyImmediate(source);
        }

        [Test]
        public void Slice_ClampsEndToClipLength()
        {
            var source = AudioClip.Create("src", 1000, 1, 1000, false);
            Assert.AreEqual(400, SpinClipSlicer.Slice(source, 0.6f, 5f).Length);
            Object.DestroyImmediate(source);
        }

        [Test]
        public void EncodeWav_Writes16BitPcmHeaderAndSamples()
        {
            var samples = new[] { 0f, 1f, -1f, 0.5f };
            var wav = SpinClipSlicer.EncodeWav(samples, 2, 22050);

            Assert.AreEqual(44 + samples.Length * 2, wav.Length);
            Assert.AreEqual("RIFF", System.Text.Encoding.ASCII.GetString(wav, 0, 4));
            Assert.AreEqual("WAVE", System.Text.Encoding.ASCII.GetString(wav, 8, 4));
            Assert.AreEqual(1, BitConverter.ToInt16(wav, 20), "PCM");
            Assert.AreEqual(2, BitConverter.ToInt16(wav, 22), "channels");
            Assert.AreEqual(22050, BitConverter.ToInt32(wav, 24), "sample rate");
            Assert.AreEqual(16, BitConverter.ToInt16(wav, 34), "bits per sample");
            Assert.AreEqual(samples.Length * 2, BitConverter.ToInt32(wav, 40), "data size");
            Assert.AreEqual(0, BitConverter.ToInt16(wav, 44));
            Assert.AreEqual(short.MaxValue, BitConverter.ToInt16(wav, 46));
            Assert.AreEqual(-short.MaxValue, BitConverter.ToInt16(wav, 48));
        }
    }
}
