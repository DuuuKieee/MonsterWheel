using MonsterWheel.Core;
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
        public void ResultSound_PlaysOnceWhenTheRingStops()
        {
            var config = ScriptableObject.CreateInstance<WheelConfig>();
            config.ResultClip = AudioClip.Create("result", 100, 1, 1000, false);
            var go = new GameObject("audio");
            var audio = go.AddComponent<SpinAudio>();
            audio.LoopSource = go.AddComponent<AudioSource>();
            audio.TailSource = go.AddComponent<AudioSource>();
            audio.ResultSource = go.AddComponent<AudioSource>();
            audio.Init(config);

            var engine = new WheelEngine(WheelSettings.Default, new SystemRandom(3));
            engine.Press();
            for (var i = 0; i < 60; i++) { engine.Tick(1f / 60f); audio.Render(engine); }
            engine.Press();
            while (engine.State != WheelState.Idle)
            {
                Assert.AreEqual(0, audio.ResultPlays, "result sound must wait for the ring to stop");
                engine.Tick(1f / 60f);
                audio.Render(engine);
            }
            Assert.AreEqual(1, audio.ResultPlays);

            for (var i = 0; i < 30; i++) audio.Render(engine); // idle frames
            Assert.AreEqual(1, audio.ResultPlays, "plays once per round");

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(config.ResultClip);
            Object.DestroyImmediate(config);
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
