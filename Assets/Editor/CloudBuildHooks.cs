using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace NorthernLands.Editor
{
    /// <summary>
    /// Generates the campaign slice, applies Android identity settings and refuses menu-bypassing builds.
    /// </summary>
    public static class CloudBuildHooks
    {
        const string k_StartupScene = "Assets/Scenes/Startup.unity";
        const string k_MainMenuScene = "Assets/Scenes/MainMenu.unity";

        public static void PreExport()
        {
            NorthernLandsRiverholmSceneBuilder.Rebuild();
            ValidateStartupScenes();

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.northernlands.game");
            PlayerSettings.bundleVersion = "0.5.0";
            PlayerSettings.Android.bundleVersionCode = 5;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Northern Lands: Riverholm, Android settings and startup menu route validated.");
        }

        static void ValidateStartupScenes()
        {
            var enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (enabledScenes.Length == 0 || !string.Equals(enabledScenes[0], k_StartupScene, StringComparison.Ordinal))
            {
                throw new BuildFailedException($"Northern Lands build must start with '{k_StartupScene}'.");
            }

            if (!enabledScenes.Contains(k_MainMenuScene, StringComparer.Ordinal))
            {
                throw new BuildFailedException($"Northern Lands build must include '{k_MainMenuScene}'.");
            }

            if (!enabledScenes.Contains(NorthernLandsRiverholmSceneBuilder.ScenePath, StringComparer.Ordinal))
            {
                throw new BuildFailedException("Northern Lands build must include the generated Riverholm scene.");
            }
        }
    }
}
