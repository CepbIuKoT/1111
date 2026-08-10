using Unity.BossRoom.Gameplay.NorthernLands.Content;
using Unity.BossRoom.Gameplay.NorthernLands.Persistence;
using VContainer.Unity;

namespace Unity.BossRoom.Gameplay.NorthernLands
{
    /// <summary>
    /// Initializes content and save state through Boss Room's existing application lifetime scope.
    /// </summary>
    public sealed class NorthernLandsRuntime : IStartable
    {
        readonly NorthernLandsContentCatalog m_Content;
        readonly NorthernLandsSaveService m_SaveService;

        public NorthernLandsRuntime(NorthernLandsContentCatalog content, NorthernLandsSaveService saveService)
        {
            m_Content = content;
            m_SaveService = saveService;
        }

        public void Start()
        {
            _ = m_Content.Races.Count;
            m_SaveService.Load();
        }
    }
}
