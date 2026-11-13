using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LoadingGame.EditorTools
{
    /// <summary>Generates the procedural UI sprites used by the modern theme.</summary>
    public static class ProceduralSprites
    {
        public const string Folder = "Assets/Sprites/UI/Generated";
        public const string PillPath = Folder + "/pill_128.png";
        public const string RingPath = Folder + "/ring_128.png";
        public const string GlowPath = Folder + "/radial_glow_512.png";
        public const string VignettePath = Folder + "/vignette_512.png";
        public const string HairlinePath = Folder + "/hairline_64.png";

        private const int PillSize = 128;
        private const float PillRadius = 64f;
        private const float RingThickness = 2f;
        private const int SoftSize = 512;
        private const float VignetteMaxAlpha = 0.45f;
        private const int HairlineSize = 64;
        private const float HairlineTopAlpha = 0.45f;

        /// <summary>Writes all sprites to disk and applies their import settings.</summary>
        public static void GenerateAll()
        {
            EnsureFolder(Folder);
            Write(PillPath, PillSize, (x, y) => PillAlpha(x, y, false));
            Write(RingPath, PillSize, (x, y) => PillAlpha(x, y, true));
            Write(GlowPath, SoftSize, GlowAlpha);
            Write(VignettePath, SoftSize, VignetteAlpha, Color.black);
            Write(HairlinePath, HairlineSize, HairlineAlpha);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var sliced = new Vector4(63f, 63f, 63f, 63f);
            AssetDatabase.StartAssetEditing();
            try
            {
                ConfigureImporter(PillPath, sliced);
                ConfigureImporter(RingPath, sliced);
                ConfigureImporter(GlowPath, Vector4.zero);
                ConfigureImporter(VignettePath, Vector4.zero);
                ConfigureImporter(HairlinePath, Vector4.zero);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        /// <summary>
        /// Writes a white copy of a glyph sprite, keeping its alpha. The game-icons.net sources are black,
        /// and a UI tint can only darken, so tinted icons need a white base.
        /// </summary>
        public static string WhiteCopy(string sourcePath)
        {
            EnsureFolder(Folder);
            string destination = Folder + "/" + Path.GetFileNameWithoutExtension(sourcePath) + "_white.png";
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            source.LoadImage(File.ReadAllBytes(sourcePath));
            Color32[] pixels = source.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(255, 255, 255, pixels[i].a);
            }
            source.SetPixels32(pixels);
            source.Apply();
            File.WriteAllBytes(destination, source.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(source);

            AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport);
            ConfigureImporter(destination, Vector4.zero);
            return destination;
        }

        /// <summary>Loads a generated sprite, failing loudly when it is missing.</summary>
        public static Sprite Load(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new InvalidOperationException("Missing generated sprite " + path);
            return sprite;
        }

        private static float PillAlpha(int x, int y, bool ring)
        {
            float half = PillSize * 0.5f;
            Vector2 p = new Vector2(x + 0.5f - half, y + 0.5f - half);
            float inner = half - PillRadius;
            Vector2 q = new Vector2(Mathf.Abs(p.x) - inner, Mathf.Abs(p.y) - inner);
            float outside = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f)).magnitude;
            float distance = outside + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - PillRadius;

            float fill = Mathf.Clamp01(0.5f - distance);
            if (!ring) return fill;
            return fill * Mathf.Clamp01(distance + RingThickness + 0.5f);
        }

        // Texture row 0 is the bottom: full alpha there, fading toward the top like the reference line.
        private static float HairlineAlpha(int x, int y)
        {
            return Mathf.Lerp(1f, HairlineTopAlpha, y / (float)(HairlineSize - 1));
        }

        private static float GlowAlpha(int x, int y)
        {
            float r = NormalizedRadius(x, y, SoftSize);
            float t = Mathf.Clamp01(1f - r);
            return t * t;
        }

        private static float VignetteAlpha(int x, int y)
        {
            float r = NormalizedRadius(x, y, SoftSize);
            return VignetteMaxAlpha * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.35f, 1.4142f, r));
        }

        private static float NormalizedRadius(int x, int y, int size)
        {
            float half = size * 0.5f;
            float dx = (x + 0.5f - half) / half;
            float dy = (y + 0.5f - half) / half;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        private static void Write(string path, int size, Func<int, int, float> alpha)
        {
            Write(path, size, alpha, Color.white);
        }

        private static void Write(string path, int size, Func<int, int, float> alpha, Color rgb)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = new Color(rgb.r, rgb.g, rgb.b, alpha(x, y));
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }

        private static void ConfigureImporter(string path, Vector4 border)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            importer.spritePixelsPerUnit = 100f;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteBorder = border;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        /// <summary>Creates every missing folder along an asset path.</summary>
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
