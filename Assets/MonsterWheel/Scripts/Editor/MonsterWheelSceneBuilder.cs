using System.IO;
using MonsterWheel.Core;
using MonsterWheel.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterWheel.EditorTools
{
    /// <summary>
    /// Builds the Monster Wheel scene, its config asset and halo texture from the art in Assets/Assets/Art and the sounds in Assets/Assets/Audio.
    /// Idempotent: running it again overwrites the generated files.
    /// </summary>
    public static class MonsterWheelSceneBuilder
    {
        private const string ArtFolder = "Assets/Assets/Art";
        private const string SpinClipPath = "Assets/Assets/Audio/freesound_community-wheel-spin-click-slow-down-101152.mp3";
        private const string ResultClipPath = "Assets/Assets/Audio/a_onclick_to_outro_magiccoin_fx.wav";
        private const string SpinLoopPath = Root + "/Audio/SpinLoop.wav";
        private const string SpinTailPath = Root + "/Audio/SpinTail.wav";
        // Steady click section of the spin sound; the slow-down tail runs from SpinLoopEnd to the end.
        private const float SpinLoopStart = 0.03f;
        private const float SpinLoopEnd = 1.30f;
        private const string Root = "Assets/MonsterWheel";
        private const string HaloPath = Root + "/Textures/Halo.png";
        private const string ConfigPath = Root + "/Config/WheelConfig.asset";
        public const string ScenePath = Root + "/Scenes/MonsterWheel.unity";

        private static readonly Vector2 ReferenceResolution = new Vector2(720f, 1280f);
        // Four evenly spaced columns between the left and right edge columns.
        private const float LeftColumnX = 110f;
        private const float RightColumnX = 610f;
        private const int ColumnCount = 4;
        private const float FirstRowY = 150f;
        private const float LastRowY = 1130f;
        private const int Rows = 7;
        private const float IconSize = 130f;
        private const float HaloSize = 200f;
        private static readonly Vector2 FrameSize = new Vector2(250f, 350f);
        private const float FrameInset = 8f;
        private const float ReelIconSize = 150f;
        private const float ReelItemHeight = 160f;
        private const int ReelItemCount = 5;

        [MenuItem("Tools/Monster Wheel/Build Scene")]
        public static void Build()
        {
            // Building opens a new scene; let the user save (or cancel) pending changes first.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            ConfigureArtImport();
            ConfigureAudioImport();
            var halo = CreateHaloSprite();
            var config = CreateConfig();
            BuildScene(config, halo);
            ConfigurePlayer();
            Debug.Log($"[MonsterWheel] Scene built at {ScenePath}");
        }

        /// <summary>Slot centers in reference pixels (origin top-left), slot 0 top-left, clockwise.</summary>
        public static Vector2[] SlotCenters()
        {
            var step = (LastRowY - FirstRowY) / (Rows - 1);
            float RowY(int r) => FirstRowY + r * step;
            var colStep = (RightColumnX - LeftColumnX) / (ColumnCount - 1);
            float ColX(int c) => LeftColumnX + c * colStep;
            var slots = new Vector2[RingLayout.SlotCount];
            var n = 0;
            for (var c = 0; c < ColumnCount; c++) slots[n++] = new Vector2(ColX(c), RowY(0));
            for (var r = 1; r < Rows - 1; r++) slots[n++] = new Vector2(RightColumnX, RowY(r));
            for (var c = ColumnCount - 1; c >= 0; c--) slots[n++] = new Vector2(ColX(c), RowY(Rows - 1));
            for (var r = Rows - 2; r >= 1; r--) slots[n++] = new Vector2(LeftColumnX, RowY(r));
            return slots;
        }

        private static void ConfigureArtImport()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ArtFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                if (path.EndsWith("frame.png")) importer.spriteBorder = new Vector4(12f, 12f, 12f, 12f);
                importer.SaveAndReimport();
            }
        }

        /// <summary>The spin clip is sliced at runtime, so its samples must be readable (decompressed on load).</summary>
        private static void ConfigureAudioImport()
        {
            if (!(AssetImporter.GetAtPath(SpinClipPath) is AudioImporter importer)) return;
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            importer.defaultSampleSettings = settings;
            importer.loadInBackground = false;
            importer.SaveAndReimport();
        }

        private static Sprite CreateHaloSprite()
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = (size - 1) / 2f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var d = Mathf.Clamp01(Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center);
                var a = (1f - d) * (1f - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
            tex.Apply();

            Directory.CreateDirectory(Path.GetDirectoryName(HaloPath));
            File.WriteAllBytes(HaloPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(HaloPath);

            var importer = (TextureImporter)AssetImporter.GetAtPath(HaloPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(HaloPath);
        }

        private static WheelConfig CreateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<WheelConfig>(ConfigPath);
            if (config == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
                config = ScriptableObject.CreateInstance<WheelConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }

            config.Icons = new Sprite[RingLayout.IconCount];
            for (var i = 0; i < RingLayout.IconCount; i++)
                config.Icons[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/icon ({i + 1}).png");

            var spin = AssetDatabase.LoadAssetAtPath<AudioClip>(SpinClipPath);
            if (spin == null)
            {
                Debug.LogWarning($"[MonsterWheel] Spin sound not found at {SpinClipPath}; the wheel will be silent.");
                config.SpinLoopClip = config.SpinTailClip = null;
                config.SpinTailLength = 0f;
            }
            else
            {
                if (spin.loadState != AudioDataLoadState.Loaded) spin.LoadAudioData();
                config.SpinLoopClip = SpinClipSlicer.WriteSlice(spin, SpinLoopStart, SpinLoopEnd, SpinLoopPath);
                config.SpinTailClip = SpinClipSlicer.WriteSlice(spin, SpinLoopEnd, spin.length, SpinTailPath);
                config.SpinTailLength = spin.length - SpinLoopEnd;
            }
            config.ResultClip = AssetDatabase.LoadAssetAtPath<AudioClip>(ResultClipPath);
            if (config.ResultClip == null)
                Debug.LogWarning($"[MonsterWheel] Result sound not found at {ResultClipPath}.");

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static void BuildScene(WheelConfig config, Sprite halo)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            camGo.transform.position = new Vector3(0f, 0f, -10f);

            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            // Expand keeps the whole 720x1280 board visible on any aspect ratio.
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var canvas = canvasGo.transform;

            // Fixed 720x1280 board centered on screen; everything else lives inside it.
            var board = NewRect("Board", canvas, ReferenceResolution, Vector2.zero);

            var bg = NewImage("Background", board, ReferenceResolution, Vector2.zero,
                AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/background.jpg"));
            bg.preserveAspect = false;

            var ring = NewRect("Ring", board, ReferenceResolution, Vector2.zero).gameObject.AddComponent<RingView>();
            var centers = SlotCenters();
            ring.Slots = new SlotView[centers.Length];
            for (var i = 0; i < centers.Length; i++)
            {
                var slotRect = NewRect($"Slot{i:00}", ring.transform, new Vector2(IconSize, IconSize), ToAnchored(centers[i]));
                var slot = slotRect.gameObject.AddComponent<SlotView>();
                slot.Halo = NewImage("Halo", slotRect, new Vector2(HaloSize, HaloSize), Vector2.zero, halo);
                slot.Icon = NewImage("Icon", slotRect, new Vector2(IconSize, IconSize), Vector2.zero, null);
                slot.Icon.preserveAspect = true;
                ring.Slots[i] = slot;
            }

            var frame = NewImage("ReelFrame", board, FrameSize, Vector2.zero,
                AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/frame.png"));
            frame.type = Image.Type.Sliced;
            var viewport = NewRect("Viewport", frame.transform, FrameSize - Vector2.one * (2f * FrameInset), Vector2.zero);
            viewport.gameObject.AddComponent<RectMask2D>();
            var reel = frame.gameObject.AddComponent<ReelView>();
            reel.ItemHeight = ReelItemHeight;
            reel.Items = new Image[ReelItemCount];
            for (var j = 0; j < ReelItemCount; j++)
            {
                reel.Items[j] = NewImage($"Item{j}", viewport, new Vector2(ReelIconSize, ReelIconSize), Vector2.zero, null);
                reel.Items[j].preserveAspect = true;
            }

            var controller = new GameObject("MonsterWheel").AddComponent<MonsterWheelController>();
            controller.Config = config;
            controller.Ring = ring;
            controller.Reel = reel;

            var audioGo = new GameObject("SpinAudio");
            var spinAudio = audioGo.AddComponent<SpinAudio>();
            spinAudio.LoopSource = audioGo.AddComponent<AudioSource>();
            spinAudio.TailSource = audioGo.AddComponent<AudioSource>();
            spinAudio.ResultSource = audioGo.AddComponent<AudioSource>();
            spinAudio.LoopSource.playOnAwake = false;
            spinAudio.TailSource.playOnAwake = false;
            spinAudio.ResultSource.playOnAwake = false;
            controller.Audio = spinAudio;

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        private static void ConfigurePlayer()
        {
            PlayerSettings.productName = "Monster Wheel";
            PlayerSettings.defaultScreenWidth = (int)ReferenceResolution.x;
            PlayerSettings.defaultScreenHeight = (int)ReferenceResolution.y;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = false;
            // Web canvas is fixed at qHD (540x960, 9:16).
            PlayerSettings.defaultWebScreenWidth = 540;
            PlayerSettings.defaultWebScreenHeight = 960;
            AssetDatabase.SaveAssets();
        }

        /// <summary>Reference pixels (origin top-left) → anchored position from the board center.</summary>
        private static Vector2 ToAnchored(Vector2 px) =>
            new Vector2(px.x - ReferenceResolution.x / 2f, ReferenceResolution.y / 2f - px.y);

        private static RectTransform NewRect(string name, Transform parent, Vector2 size, Vector2 anchored)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchored;
            return rect;
        }

        private static Image NewImage(string name, Transform parent, Vector2 size, Vector2 anchored, Sprite sprite)
        {
            var rect = NewRect(name, parent, size, anchored);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }
    }
}
