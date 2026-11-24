#if UNITY_EDITOR
using Code.Game.StaticData.Configs;
using Code.Infrastructure.DI.LifetimeScopes;
using Code.Meta.UI.MainMenu;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Code.Infrastructure.Editor
{
    public static class LevelSetupUtility
    {
        private const string GameScenePath = "Assets/Scenes/Game.unity";
        private const string LevelPrefabPath = "Assets/Prefabs/Level/Level1/Level_01.prefab";
        private const string LevelConfigPath = "Assets/Configs/Game/Levels/Level1/LevelConfig.asset";
        private const string LevelCatalogPath = "Assets/Configs/Game/Levels/LevelCatalog.asset";
        private const string WavesConfigPath = "Assets/Configs/Game/Levels/Level1/WavesConfig.asset";
        private const string GlobalScopePrefabPath = "Assets/Settings/DI/GlobalLifetimeScope.prefab";
        private const string MainMenuPrefabPath = "Assets/Prefabs/Menu/MainMenu.prefab";

        private static readonly string[] LevelRootNames =
        {
            "LevelTileMap",
            "EnemySpawnPoints",
            "PlayerSpawnPoints",
            "Environment",
            "Castle",
            "Tower",
            "Tower (1)",
            "Tower (2)",
            "Tower (3)",
            "Global Light 2D"
        };

        [MenuItem("Tools/Levels/Setup Level Prefab Flow")]
        public static void SetupFromMenu() => Setup();

        public static void Setup()
        {
            EnsureFolders();

            var levelPrefab = ExtractOrLoadLevelPrefab();
            var levelConfig = CreateOrUpdateLevelConfig(levelPrefab);
            var catalog = CreateOrUpdateLevelCatalog(levelConfig);

            WireGlobalScope(catalog);
            WireMainMenu();
            WireGameScene(levelPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[LevelSetupUtility] Level prefab flow setup complete.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Prefabs/Level");
            EnsureFolder("Assets/Prefabs/Level/Level1");
            EnsureFolder("Assets/Configs/Game/Levels");
            EnsureFolder("Assets/Configs/Game/Levels/Level1");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parts = path.Split('/');
            var current = parts[0];

            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static GameObject ExtractOrLoadLevelPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(LevelPrefabPath);
            if (existing != null)
                return existing;

            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var move = new List<GameObject>();

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "GameSceneScope")
                {
                    CollectLevelChildren(root.transform, move);
                    continue;
                }

                if (IsLevelObjectName(root.name))
                    move.Add(root);
            }

            if (move.Count == 0)
            {
                Debug.LogError("[LevelSetupUtility] No level objects found in Game scene.");
                return null;
            }

            var levelRoot = new GameObject("Level_01");
            foreach (var go in move)
                go.transform.SetParent(levelRoot.transform, true);

            var prefab = PrefabUtility.SaveAsPrefabAsset(levelRoot, LevelPrefabPath);
            Object.DestroyImmediate(levelRoot);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            return prefab;
        }

        private static void CollectLevelChildren(Transform scope, List<GameObject> move)
        {
            for (var i = scope.childCount - 1; i >= 0; i--)
            {
                var child = scope.GetChild(i).gameObject;
                if (IsLevelObjectName(child.name))
                    move.Add(child);
            }
        }

        private static bool IsLevelObjectName(string name)
        {
            for (var i = 0; i < LevelRootNames.Length; i++)
            {
                if (LevelRootNames[i] == name)
                    return true;
            }

            return name.StartsWith("Tower");
        }

        private static LevelConfig CreateOrUpdateLevelConfig(GameObject levelPrefab)
        {
            var config = AssetDatabase.LoadAssetAtPath<LevelConfig>(LevelConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<LevelConfig>();
                AssetDatabase.CreateAsset(config, LevelConfigPath);
            }

            var waves = AssetDatabase.LoadAssetAtPath<WavesConfig>(WavesConfigPath);
            var so = new SerializedObject(config);
            so.FindProperty("<Id>k__BackingField").stringValue = "level_01";
            so.FindProperty("<Title>k__BackingField").stringValue = "Level 1";
            so.FindProperty("<LevelPrefab>k__BackingField").objectReferenceValue = levelPrefab;
            so.FindProperty("<Waves>k__BackingField").objectReferenceValue = waves;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);

            return config;
        }

        private static LevelCatalog CreateOrUpdateLevelCatalog(LevelConfig levelConfig)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(LevelCatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelCatalog>();
                AssetDatabase.CreateAsset(catalog, LevelCatalogPath);
            }

            if (catalog.Count == 0)
            {
                catalog.SetNodes(LevelCatalogDefaults.CreateNodes(levelConfig));
                EditorUtility.SetDirty(catalog);
            }

            return catalog;
        }

        private static void WireGlobalScope(LevelCatalog catalog)
        {
            var root = PrefabUtility.LoadPrefabContents(GlobalScopePrefabPath);
            var scope = root.GetComponent<GlobalScope>();
            if (scope == null)
            {
                PrefabUtility.UnloadPrefabContents(root);
                Debug.LogError("[LevelSetupUtility] GlobalScope not found.");
                return;
            }

            var so = new SerializedObject(scope);
            so.FindProperty("_levelCatalog").objectReferenceValue = catalog;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, GlobalScopePrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void WireMainMenu()
        {
            var root = PrefabUtility.LoadPrefabContents(MainMenuPrefabPath);
            var screen = root.GetComponent<MainMenuScreen>();
            var doc = root.GetComponent<UIDocument>();
            var select = root.GetComponent<LevelSelectMenu>();

            if (select == null)
                select = root.AddComponent<LevelSelectMenu>();

            var selectSo = new SerializedObject(select);
            selectSo.FindProperty("_mainMenuDoc").objectReferenceValue = doc;
            selectSo.ApplyModifiedPropertiesWithoutUndo();

            if (screen != null)
            {
                var screenSo = new SerializedObject(screen);
                screenSo.FindProperty("_levelSelectMenu").objectReferenceValue = select;
                screenSo.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, MainMenuPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void WireGameScene(GameObject levelPrefab)
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            GameObject scopeGo = null;

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "GameSceneScope")
                {
                    scopeGo = root;
                    break;
                }
            }

            if (scopeGo == null)
            {
                Debug.LogError("[LevelSetupUtility] GameSceneScope not found.");
                return;
            }

            RemoveRemainingLevelObjects(scene, scopeGo.transform);

            var levelRootTransform = scopeGo.transform.Find("LevelRoot");
            if (levelRootTransform == null)
            {
                var levelRootGo = new GameObject("LevelRoot");
                levelRootGo.transform.SetParent(scopeGo.transform, false);
                levelRootTransform = levelRootGo.transform;
            }

            var scope = scopeGo.GetComponent<GameSceneScope>();
            var so = new SerializedObject(scope);
            so.FindProperty("_levelRoot").objectReferenceValue = levelRootTransform;

            var wavesProp = so.FindProperty("_wavesConfig");
            if (wavesProp != null)
                so.FindProperty("_wavesConfig").objectReferenceValue = null;

            var autoInject = so.FindProperty("autoInjectGameObjects");
            if (autoInject != null)
            {
                for (var i = autoInject.arraySize - 1; i >= 0; i--)
                {
                    var go = autoInject.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                    if (go == null || IsLevelObjectName(go.name))
                        autoInject.DeleteArrayElementAtIndex(i);
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void RemoveRemainingLevelObjects(Scene scene, Transform scope)
        {
            for (var i = scope.childCount - 1; i >= 0; i--)
            {
                var child = scope.GetChild(i).gameObject;
                if (child.name == "LevelRoot")
                    continue;

                if (IsLevelObjectName(child.name))
                    Object.DestroyImmediate(child);
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "GameSceneScope")
                    continue;

                if (IsLevelObjectName(root.name))
                    Object.DestroyImmediate(root);
            }
        }
    }
}
#endif
