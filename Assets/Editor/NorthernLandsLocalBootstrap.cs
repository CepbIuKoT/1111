using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NorthernLands.Editor
{
    /// <summary>
    /// Prepares the generated campaign once after the project is opened locally.
    /// Cloud builds keep using CloudBuildHooks.PreExport instead.
    /// </summary>
    [InitializeOnLoad]
    public static class NorthernLandsLocalBootstrap
    {
        const string k_StartupScene = "Assets/Scenes/Startup.unity";
        const string k_SessionKey = "NorthernLands.LocalBootstrap.Attempted";

        static readonly string[] s_GeneratedScenes =
        {
            NorthernLandsRiverholmSceneBuilder.ScenePath,
            NorthernLandsRiverholmSceneBuilder.DeadWorldScenePath,
            NorthernLandsRiverholmSceneBuilder.TowerScenePath
        };

        static NorthernLandsLocalBootstrap()
        {
            if (Application.isBatchMode || SessionState.GetBool(k_SessionKey, false))
            {
                return;
            }

            EditorApplication.update += PrepareWhenEditorIsReady;
        }

        [MenuItem("Northern Lands/Prepare Local Playable Project")]
        public static void PrepareNow()
        {
            PrepareGeneratedCampaign(true);
        }

        static void PrepareWhenEditorIsReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                return;
            }

            EditorApplication.update -= PrepareWhenEditorIsReady;
            SessionState.SetBool(k_SessionKey, true);
            PrepareGeneratedCampaign(false);
        }

        static void PrepareGeneratedCampaign(bool forceRebuild)
        {
            if (!forceRebuild && s_GeneratedScenes.All(File.Exists))
            {
                return;
            }

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.isDirty)
            {
                Debug.LogWarning(
                    "Northern Lands: automatic scene generation was skipped because the active scene has unsaved changes. " +
                    "Save it, then use Northern Lands > Prepare Local Playable Project.");
                return;
            }

            try
            {
                NorthernLandsRiverholmSceneBuilder.Rebuild();
                EditorSceneManager.OpenScene(k_StartupScene, OpenSceneMode.Single);
                Debug.Log("Northern Lands: local campaign scenes are ready. Play Mode will start from the main menu.");
            }
            catch (Exception exception)
            {
                Debug.LogError("Northern Lands: local campaign preparation failed. See the exception below.");
                Debug.LogException(exception);
            }
        }
    }
}
