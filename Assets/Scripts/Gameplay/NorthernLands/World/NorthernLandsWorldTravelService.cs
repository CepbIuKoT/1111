using Unity.BossRoom.Gameplay.NorthernLands.Content;
using Unity.BossRoom.Gameplay.NorthernLands.GameState;
using Unity.BossRoom.Gameplay.NorthernLands.Persistence;

namespace Unity.BossRoom.Gameplay.NorthernLands.World
{
    public sealed class NorthernLandsWorldTravelService
    {
        readonly NorthernLandsContentCatalog m_Content;
        readonly NorthernLandsProgressState m_Progress;
        readonly NorthernLandsSaveService m_Save;

        public NorthernLandsWorldTravelService(NorthernLandsContentCatalog content, NorthernLandsProgressState progress, NorthernLandsSaveService save)
        {
            m_Content = content;
            m_Progress = progress;
            m_Save = save;
        }

        public bool TryTravel(NorthernWorldId destination, RaceAbilityKind? activeRaceAbility, out string sceneName)
        {
            sceneName = null;
            if (!m_Progress.CanEnter(destination, activeRaceAbility))
            {
                return false;
            }

            var world = m_Content.GetWorld(destination);
            m_Progress.EnterWorld(destination);
            m_Save.SaveRun();
            sceneName = world.sceneName;
            return true;
        }
    }
}
