using System;
using System.Collections.Generic;
using UnityEngine;

namespace NorthernLands.SaveSystem
{
    [Serializable]
    public sealed class SaveGameData
    {
        public const int CurrentFormatVersion = 1;

        public int formatVersion = CurrentFormatVersion;
        public string currentScene = "01_MainMenu";
        public Vector3Data safePosition = new();
        public int playerLevel = 1;
        public int experience;
        public int gold;
        public List<string> selectedTalentIds = new();
        public List<string> completedQuestIds = new();
        public long savedAtUnixUtc;

        public void TouchTimestamp()
        {
            savedAtUnixUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    [Serializable]
    public sealed class Vector3Data
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3() => new(x, y, z);

        public void Set(Vector3 value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }
    }
}
