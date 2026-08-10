using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NorthernLands.Editor
{
    /// <summary>
    /// Applies Android identity settings and refuses to build an APK that bypasses the startup menu.
    /// </summary>
    public static class CloudBuildHooks
    {
        const string k_StartupScene = "Assets/Scenes/Startup.unity";
        const string k_MainMenuScene = "Assets/Scenes/MainMenu.unity";

        public static void PreExport()
        {
            ValidateStartupScenes();

            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Android,
                "com.northernlands.game");
            PlayerSettings.bundleVersion = "0.4.0";
            PlayerSettings.Android.bundleVersionCode = 4;
            PlayerSettings.defaultInterfaceOrientation =
                UIOrientation.LandscapeLeft;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                "Northern Lands: Android settings and startup menu route validated.");
        }

        static void ValidateStartupScenes()
        {
            var enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (enabledScenes.Length == 0 ||
                !string.Equals(enabledScenes[0], k_StartupScene, StringComparison.Ordinal))
            {
                throw new BuildFailedException(
                    $"Northern Lands build must start with '{k_StartupScene}'.");
            }

            if (!enabledScenes.Contains(k_MainMenuScene, StringComparer.Ordinal))
            {
                throw new BuildFailedException(
                    $"Northern Lands build must include '{k_MainMenuScene}'.");
            }
        }
    }
}
