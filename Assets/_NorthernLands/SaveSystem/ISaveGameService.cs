using NorthernLands.Core.Services;

namespace NorthernLands.SaveSystem
{
    public interface ISaveGameService : IGameService
    {
        bool HasSave { get; }
        SaveGameData LoadOrCreate();
        void Save(SaveGameData data);
        void DeleteRunSave();
    }
}
