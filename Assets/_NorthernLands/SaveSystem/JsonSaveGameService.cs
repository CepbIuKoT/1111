using System;
using System.IO;
using UnityEngine;

namespace NorthernLands.SaveSystem
{
    public sealed class JsonSaveGameService : ISaveGameService
    {
        private const string SaveFileName = "northern_lands_save.json";
        private const string BackupFileName = "northern_lands_save.backup.json";

        private string _savePath;
        private string _backupPath;

        public bool HasSave => File.Exists(_savePath);

        public void Initialize()
        {
            _savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
            _backupPath = Path.Combine(Application.persistentDataPath, BackupFileName);
        }

        public SaveGameData LoadOrCreate()
        {
            if (!HasSave)
                return new SaveGameData();

            try
            {
                var json = File.ReadAllText(_savePath);
                var data = JsonUtility.FromJson<SaveGameData>(json);
                return Validate(data) ? data : LoadBackupOrCreate();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Primary save could not be loaded: {exception.Message}");
                return LoadBackupOrCreate();
            }
        }

        public void Save(SaveGameData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            data.formatVersion = SaveGameData.CurrentFormatVersion;
            data.TouchTimestamp();
            var json = JsonUtility.ToJson(data, true);
            var temporaryPath = _savePath + ".tmp";

            File.WriteAllText(temporaryPath, json);
            if (File.Exists(_savePath))
                File.Copy(_savePath, _backupPath, true);

            File.Copy(temporaryPath, _savePath, true);
            File.Delete(temporaryPath);
        }

        public void DeleteRunSave()
        {
            DeleteIfExists(_savePath);
            DeleteIfExists(_backupPath);
        }

        public void Shutdown()
        {
        }

        private SaveGameData LoadBackupOrCreate()
        {
            if (!File.Exists(_backupPath))
                return new SaveGameData();

            try
            {
                var data = JsonUtility.FromJson<SaveGameData>(File.ReadAllText(_backupPath));
                return Validate(data) ? data : new SaveGameData();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Backup save could not be loaded: {exception.Message}");
                return new SaveGameData();
            }
        }

        private static bool Validate(SaveGameData data)
        {
            return data != null
                && data.formatVersion > 0
                && data.formatVersion <= SaveGameData.CurrentFormatVersion
                && data.playerLevel >= 1;
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
