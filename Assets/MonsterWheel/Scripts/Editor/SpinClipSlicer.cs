using System;
using System.IO;
using UnityEngine;

namespace MonsterWheel.EditorTools
{
    /// <summary>
    /// Cuts the spin sound into loop / tail WAV assets at edit time. Runtime slicing is not possible on
    /// WebGL (no AudioClip.GetData / PCM AudioClip.Create there).
    /// </summary>
    public static class SpinClipSlicer
    {
        /// <summary>Interleaved samples of [start, end) seconds of <paramref name="source"/>; end is clamped to the clip.</summary>
        public static float[] Slice(AudioClip source, float start, float end)
        {
            var frequency = source.frequency;
            var first = Mathf.Clamp(Mathf.RoundToInt(start * frequency), 0, source.samples - 1);
            var last = Mathf.Clamp(Mathf.RoundToInt(end * frequency), first + 1, source.samples);
            var data = new float[(last - first) * source.channels];
            source.GetData(data, first);
            return data;
        }

        /// <summary>16-bit PCM WAV file bytes for interleaved <paramref name="samples"/>.</summary>
        public static byte[] EncodeWav(float[] samples, int channels, int frequency)
        {
            const int bytesPerSample = 2;
            var dataSize = samples.Length * bytesPerSample;
            using (var stream = new MemoryStream(44 + dataSize))
            using (var w = new BinaryWriter(stream))
            {
                w.Write(new[] { 'R', 'I', 'F', 'F' });
                w.Write(36 + dataSize);
                w.Write(new[] { 'W', 'A', 'V', 'E' });
                w.Write(new[] { 'f', 'm', 't', ' ' });
                w.Write(16);
                w.Write((short)1); // PCM
                w.Write((short)channels);
                w.Write(frequency);
                w.Write(frequency * channels * bytesPerSample);
                w.Write((short)(channels * bytesPerSample));
                w.Write((short)(bytesPerSample * 8));
                w.Write(new[] { 'd', 'a', 't', 'a' });
                w.Write(dataSize);
                foreach (var s in samples)
                    w.Write((short)Math.Round(Mathf.Clamp(s, -1f, 1f) * short.MaxValue));
                return stream.ToArray();
            }
        }

        /// <summary>Writes [start, end) of <paramref name="source"/> to a WAV asset and returns the imported clip.</summary>
        public static AudioClip WriteSlice(AudioClip source, float start, float end, string assetPath)
        {
            var samples = Slice(source, start, end);
            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            File.WriteAllBytes(assetPath, EncodeWav(samples, source.channels, source.frequency));
            UnityEditor.AssetDatabase.ImportAsset(assetPath, UnityEditor.ImportAssetOptions.ForceUpdate);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        }
    }
}
