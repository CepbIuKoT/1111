using System;
using System.IO;
using Unity.BossRoom.Gameplay.NorthernLands.GameState;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.NorthernLands.Persistence
{
    /// <summary>
    /// Versioned local persistence. The eternal race is deliberately stored separately from run progress.
    /// </summary>
    public sealed class NorthernLandsSaveService
    {
        const string k_RunFileName = "northern-realms-v14-living-start.json";
        const string k_RaceFileName = "northern-realms-eternal-race-v1.json";

        readonly NorthernLandsProgressState m_Progress;

        string RunPath => Path.Combine(Application.persistentDataPath, k_RunFileName);
        string RacePath => Path.Combine(Application.persistentDataPath, k_RaceFileName);

        public NorthernLandsSaveService(NorthernLandsProgressState progress)
        {
            m_Progress = progress;
        }

        public void Load()
        {
            var run = ReadOrDefault<NorthernLandsSaveData>(RunPath);
            var race = ReadOrDefault<EternalRaceSaveData>(RacePath);
            m_Progress.Restore(run, race);
        }

        public void SaveRun()
        {
            WriteAtomically(RunPath, JsonUtility.ToJson(m_Progress.Run, true));
        }

        public void SavePermanentRace()
        {
            WriteAtomically(RacePath, JsonUtility.ToJson(m_Progress.EternalRace, true));
        }

        public void ResetRunKeepingRace()
        {
            m_Progress.Restore(null, m_Progress.EternalRace);
            SaveRun();
        }

        static T ReadOrDefault<T>(string path) where T : new()
        {
            if (!File.Exists(path))
            {
                return new T();
            }

            try
            {
                var result = JsonUtility.FromJson<T>(File.ReadAllText(path));
                return result == null ? new T() : result;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Northern Lands save could not be read from '{path}': {exception.Message}");
                return new T();
            }
        }

        static void WriteAtomically(string path, string json)
        {
            var temporaryPath = path + ".tmp";
            var backupPath = path + ".bak";
            File.WriteAllText(temporaryPath, json);

            if (File.Exists(path))
            {
                try
                {
                    File.Replace(temporaryPath, path, backupPath);
                }
                catch (PlatformNotSupportedException)
                {
                    File.Copy(path, backupPath, true);
                    File.Delete(path);
                    File.Move(temporaryPath, path);
                }
            }
            else
            {
                File.Move(temporaryPath, path);
            }
        }
    }
}
