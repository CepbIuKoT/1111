using System.Collections.Generic;
using System.IO;
using System.Linq;
using NorthernLands.Core.Bootstrap;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NorthernLands.Editor
{
    public static class NorthernLandsProjectSetup
    {
        private const string Root = "Assets/_NorthernLands";
        private const string ScenesRoot = Root + "/Scenes";

        private static readonly string[] FolderPaths =
        {
            "Art/Characters", "Art/Creatures", "Art/Environments", "Art/Props",
            "Art/Materials", "Art/Textures", "Art/UI", "Audio/Music",
            "Audio/Ambience", "Audio/SFX", "Audio/Voice", "Animations/Player",
            "Animations/Enemies", "Animations/NPC", "Animations/Bosses",
            "Core/Events", "Core/Utilities", "GameData/Abilities", "GameData/Items",
            "GameData/LootTables", "GameData/Enemies", "GameData/NPC",
            "GameData/Bosses", "GameData/Quests", "GameData/Dialogues",
            "GameData/Worlds", "Camera", "Combat", "Abilities", "Progression",
            "AI", "Inventory", "Quests", "Dialogue", "Reputation", "DeathCycle",
            "World/Streaming", "World/Portals", "World/Dungeons", "World/Towns",
            "World/SpawnRules", "UI/HUD", "UI/MobileControls", "UI/Inventory",
            "UI/Talents", "UI/Quests", "UI/Dialogue", "UI/Menus", "VFX",
            "Prefabs", "Scenes", "Tests/PlayMode", "Tests/Device"
        };

        private static readonly string[] SceneNames =
        {
            "00_Bootstrap", "01_MainMenu", "02_RaceSelection", "10_NorthernLands",
            "11_Riverholm", "20_AshWorld", "21_AshHarbor", "30_StarWastes",
            "31_Astralis", "40_DeadWorld", "41_TowerOfGods", "50_AncientDungeon",
            "60_SilentDimension", "90_CombatSandbox", "91_AIPlayground",
            "92_MobileUITest", "93_SaveTest", "94_RaceAbilityTest",
            "95_PerformanceBenchmark"
        };

        [MenuItem("Tools/Northern Lands/1. Create Project Folders")]
        public static void CreateFolders()
        {
            foreach (var relativePath in FolderPaths)
                Directory.CreateDirectory(Path.Combine(Root, relativePath));

            AssetDatabase.Refresh();
            Debug.Log("Northern Lands: project folders are ready.");
        }

        [MenuItem("Tools/Northern Lands/2. Create Starter Scenes")]
        public static void CreateStarterScenes()
        {
            CreateFolders();
            var buildScenes = new List<EditorBuildSettingsScene>();

            foreach (var sceneName in SceneNames)
            {
                var scenePath = $"{ScenesRoot}/{sceneName}.unity";
                if (!File.Exists(scenePath))
                {
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    CreateDefaultSceneObjects(sceneName);
                    EditorSceneManager.SaveScene(scene, scenePath);
                }

                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Northern Lands: {SceneNames.Length} scenes are ready and included in Build Settings.");
        }

        [MenuItem("Tools/Northern Lands/3. Apply Android Defaults")]
        public static void ApplyAndroidDefaults()
        {
            PlayerSettings.companyName = "Northern Lands Studio";
            PlayerSettings.productName = "Северные Земли XIV";
            PlayerSettings.bundleVersion = "0.2.0";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.northernlands.game");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            AssetDatabase.SaveAssets();
            Debug.Log("Northern Lands: Android defaults applied (IL2CPP, ARM64, landscape, API 26+).");
        }

        [MenuItem("Tools/Northern Lands/Validate Foundation")]
        public static void ValidateFoundation()
        {
            var missing = FolderPaths
                .Select(path => Path.Combine(Root, path))
                .Where(path => !Directory.Exists(path))
                .ToList();

            missing.AddRange(SceneNames
                .Select(name => $"{ScenesRoot}/{name}.unity")
                .Where(path => !File.Exists(path)));

            if (missing.Count == 0)
            {
                Debug.Log("Northern Lands foundation validation passed.");
                return;
            }

            Debug.LogWarning("Northern Lands foundation is incomplete:\n" + string.Join("\n", missing));
        }

        private static void CreateDefaultSceneObjects(string sceneName)
        {
            if (sceneName == "00_Bootstrap")
            {
                var bootstrap = new GameObject("GameBootstrap");
                bootstrap.AddComponent<GameBootstrap>();
                return;
            }

            var marker = new GameObject($"{sceneName}_ROOT");
            SceneManager.MoveGameObjectToScene(marker, SceneManager.GetActiveScene());
        }
    }
}
