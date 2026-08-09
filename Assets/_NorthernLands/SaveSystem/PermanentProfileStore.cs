using System;
using System.IO;
using UnityEngine;

namespace NorthernLands.SaveSystem
{
    /// <summary>
    /// Stores choices that must survive an ordinary run reset. There is deliberately
    /// no public delete method in the runtime interface.
    /// </summary>
    public sealed class PermanentProfileStore : IPermanentProfileStore
    {
        private const string FileName = "northern_lands_profile.json";
        private string _path;

        public void Initialize()
        {
            _path = Path.Combine(Application.persistentDataPath, FileName);
        }

        public PermanentProfileData Load()
        {
            if (!File.Exists(_path))
                return new PermanentProfileData();

            try
            {
                return JsonUtility.FromJson<PermanentProfileData>(File.ReadAllText(_path))
                       ?? new PermanentProfileData();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Permanent profile could not be loaded: {exception.Message}");
                return new PermanentProfileData();
            }
        }

        public bool TryLockRace(string raceId)
        {
            if (string.IsNullOrWhiteSpace(raceId))
                throw new ArgumentException("Race id is required.", nameof(raceId));

            var data = Load();
            if (data.raceLocked)
                return string.Equals(data.permanentRaceId, raceId, StringComparison.Ordinal);

            data.raceLocked = true;
            data.permanentRaceId = raceId;
            data.createdAtUnixUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            File.WriteAllText(_path, JsonUtility.ToJson(data, true));
            return true;
        }

        public void Shutdown()
        {
        }
    }
}
