using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace NorthernLands.Editor
{
    /// <summary>
    /// Prepares a self-contained combat sandbox before Unity Build Automation
    /// exports the Android player. The generated scene intentionally uses only
    /// Unity primitives, so the first cloud APK does not depend on Asset Store
    /// packages or a local Unity workstation.
    /// </summary>
    public static class CloudBuildHooks
    {
        private const string CombatSandboxPath =
            "Assets/_NorthernLands/Scenes/90_CombatSandbox.unity";

        public static void PreExport()
        {
            Debug.Log("Northern Lands: preparing cloud Android build.");

            NorthernLandsProjectSetup.CreateFolders();
            NorthernLandsProjectSetup.ApplyAndroidDefaults();

            // Build Automation can reuse a cached workspace. Recreate the
            // generated scene deterministically and avoid the interactive
            // overwrite dialog used by the normal Editor menu command.
            if (File.Exists(CombatSandboxPath))
                AssetDatabase.DeleteAsset(CombatSandboxPath);

            CombatSandboxSetup.CreateCombatSandbox();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(CombatSandboxPath, true)
            };

            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Android,
                "com.northernlands.game");
            PlayerSettings.bundleVersion = "0.2.0";
            PlayerSettings.Android.bundleVersionCode = 2;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Northern Lands: cloud build scene and Android settings are ready.");
        }
    }
}
