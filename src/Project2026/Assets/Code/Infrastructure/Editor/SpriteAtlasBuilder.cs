#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Code.Infrastructure.Editor
{
    /// <summary>
    /// Builds the game's Sprite Atlases (V2) from what the content actually uses, so sprites drawn together share
    /// one texture and batch. Re-runnable: atlases are regenerated from scratch each time.
    ///  - Battle: units, projectiles, hit / dust / debuff effects, range circle.
    ///  - Environment: level prefabs (tiles, decor, buildings), castle, tower, the Town scene.
    /// Sprites are collected from each root's dependencies (prefabs → animator controllers → clips → sprites,
    /// tilemaps → tiles → sprites). A texture goes to one atlas only (Battle wins). Textures that are only used
    /// by materials (particles, UI) are skipped: packing them would not change how they are drawn.
    /// </summary>
    public static class SpriteAtlasBuilder
    {
        private const string AtlasFolder = "Assets/Art/Atlases";

        private static readonly string[] BattleRoots =
        {
            "Assets/Prefabs/Defenders",
            "Assets/Prefabs/Enemies",
            "Assets/Prefabs/Projectiles",
            "Assets/Prefabs/Effects/Hits",
            "Assets/Prefabs/Effects/Dust",
            "Assets/Prefabs/Effects/Dead",
            "Assets/Prefabs/Effects/Debaff",
            "Assets/Prefabs/RangeViewService.prefab",
        };

        private static readonly string[] EnvironmentRoots =
        {
            "Assets/Prefabs/Level",
            "Assets/Prefabs/Castle",
            "Assets/Prefabs/Tower",
            "Assets/Scenes/Town.unity",
        };

        private static readonly string[] ExcludedPrefixes =
        {
            "Assets/UI Toolkit/",
            "Assets/TextMesh Pro/",
            "Packages/",
        };

        [MenuItem("Tools/Rendering/Rebuild Sprite Atlases", false, 10)]
        public static void Rebuild()
        {
            var battle = CollectSpriteTextures(BattleRoots);
            var environment = CollectSpriteTextures(EnvironmentRoots);
            environment.ExceptWith(battle);

            Directory.CreateDirectory(AtlasFolder);

            var battleAtlas = CreateAtlas("Battle", battle);
            var environmentAtlas = CreateAtlas("Environment", environment);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var atlases = new[] { battleAtlas, environmentAtlas }
                .Select(AssetDatabase.LoadAssetAtPath<SpriteAtlas>)
                .Where(a => a != null)
                .ToArray();

            SpriteAtlasUtility.PackAtlases(atlases, EditorUserBuildSettings.activeBuildTarget);

            foreach (var atlas in atlases)
                Debug.Log($"[SpriteAtlasBuilder] {atlas.name}: {atlas.spriteCount} sprites.");
        }

        private static string CreateAtlas(string name, HashSet<string> texturePaths)
        {
            var path = $"{AtlasFolder}/{name}.spriteatlasv2";

            if (File.Exists(path))
                AssetDatabase.DeleteAsset(path);

            var packables = texturePaths
                .OrderBy(p => p)
                .Select(AssetDatabase.LoadAssetAtPath<Texture2D>)
                .Where(t => t != null)
                .Cast<Object>()
                .ToArray();

            var asset = new SpriteAtlasAsset();
            asset.Add(packables);
            SpriteAtlasAsset.Save(asset, path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            var importer = (SpriteAtlasImporter)AssetImporter.GetAtPath(path);

            importer.includeInBuild = true;
            importer.packingSettings = new SpriteAtlasPackingSettings
            {
                blockOffset = 1,
                padding = 4,
                enableRotation = false,
                enableTightPacking = true,
                enableAlphaDilation = true,
            };
            importer.textureSettings = new SpriteAtlasTextureSettings
            {
                filterMode = FilterMode.Bilinear,
                generateMipMaps = false,
                readable = false,
                sRGB = true,
                anisoLevel = 0,
            };

            importer.SetPlatformSettings(new TextureImporterPlatformSettings
            {
                name = "DefaultTexturePlatform",
                maxTextureSize = 4096,
                textureCompression = TextureImporterCompression.Compressed,
            });
            importer.SetPlatformSettings(new TextureImporterPlatformSettings
            {
                name = "Standalone",
                overridden = true,
                maxTextureSize = 4096,
                format = TextureImporterFormat.BC7,
                textureCompression = TextureImporterCompression.CompressedHQ,
            });
            importer.SetPlatformSettings(new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = 2048,
                format = TextureImporterFormat.ASTC_6x6,
                textureCompression = TextureImporterCompression.Compressed,
            });

            importer.SaveAndReimport();
            Debug.Log($"[SpriteAtlasBuilder] {name}: {packables.Length} textures → {path}");
            return path;
        }

        private static HashSet<string> CollectSpriteTextures(string[] roots)
        {
            var rootAssets = new List<string>();

            foreach (var root in roots)
            {
                if (AssetDatabase.IsValidFolder(root))
                {
                    rootAssets.AddRange(AssetDatabase.FindAssets("t:Prefab", new[] { root })
                        .Select(AssetDatabase.GUIDToAssetPath));
                }
                else if (File.Exists(root))
                {
                    rootAssets.Add(root);
                }
            }

            var dependencies = AssetDatabase.GetDependencies(rootAssets.ToArray(), true);

            // Textures reached through materials (particles, UI) are drawn with that material, not as sprites.
            var materialTextures = new HashSet<string>(AssetDatabase.GetDependencies(
                dependencies.Where(d => d.EndsWith(".mat")).ToArray(), true));

            var result = new HashSet<string>();

            foreach (var path in dependencies)
            {
                if (ExcludedPrefixes.Any(path.StartsWith) || materialTextures.Contains(path))
                    continue;

                if (AssetImporter.GetAtPath(path) is TextureImporter { textureType: TextureImporterType.Sprite })
                    result.Add(path);
            }

            return result;
        }
    }
}
#endif
