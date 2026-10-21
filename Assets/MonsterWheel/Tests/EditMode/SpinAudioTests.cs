using MonsterWheel.Core;
using MonsterWheel.View;
using NUnit.Framework;
using UnityEngine;

namespace MonsterWheel.Tests
{
    public class SpinAudioTests
    {
        [Test]
        public void Init_UsesPreSlicedClipsAsIs()
        {
            // WebGL cannot read or create PCM data at runtime, so SpinAudio must only play ready-made clips.
            var config = ScriptableObject.CreateInstance<WheelConfig>();
            config.SpinLoopClip = AudioClip.Create("loop", 100, 1, 1000, false);
            config.SpinTailClip = AudioClip.Create("tail", 50, 1, 1000, false);
            config.SpinTailLength = 1.1f;
            var go = new GameObject("audio");
            var audio = go.AddComponent<SpinAudio>();
            audio.LoopSource = go.AddComponent<AudioSource>();
            audio.TailSource = go.AddComponent<AudioSource>();
            audio.ResultSource = go.AddComponent<AudioSource>();

            audio.Init(config);

            Assert.AreSame(config.SpinLoopClip, audio.LoopSource.clip);
            Assert.AreSame(config.SpinTailClip, audio.TailSource.clip);
            Assert.IsTrue(audio.LoopSource.loop);
            Assert.IsFalse(audio.TailSource.loop);
            Assert.AreEqual(1.1f, audio.TailLength, "tail length comes from config, not from the (maybe unloaded) clip");

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(config.SpinLoopClip);
            Object.DestroyImmediate(config.SpinTailClip);
            Object.DestroyImmediate(config);
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
    }
}
