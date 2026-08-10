using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace NorthernLands.Editor
{
    /// <summary>
    /// Keeps the official Boss Room scenes and gameplay intact while
    /// applying Android identity settings for Unity Build Automation.
    /// </summary>
    public static class CloudBuildHooks
    {
        public static void PreExport()
        {
            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Android,
                "com.northernlands.game");
            PlayerSettings.bundleVersion = "0.3.0";
            PlayerSettings.Android.bundleVersionCode = 3;
            PlayerSettings.defaultInterfaceOrientation =
                UIOrientation.LandscapeLeft;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                "Northern Lands: Boss Room Android build settings applied.");
        }
    }
}
