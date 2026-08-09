using System;

namespace NorthernLands.SaveSystem
{
    [Serializable]
    public sealed class PermanentProfileData
    {
        public int formatVersion = 1;
        public bool raceLocked;
        public string permanentRaceId = string.Empty;
        public long createdAtUnixUtc;
    }
}
