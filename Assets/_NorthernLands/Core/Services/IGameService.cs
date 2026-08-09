namespace NorthernLands.Core.Services
{
    /// <summary>Lifecycle contract for persistent game services.</summary>
    public interface IGameService
    {
        void Initialize();
        void Shutdown();
    }
}
