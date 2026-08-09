using NorthernLands.Core.Services;

namespace NorthernLands.SaveSystem
{
    public interface IPermanentProfileStore : IGameService
    {
        PermanentProfileData Load();
        bool TryLockRace(string raceId);
    }
}
