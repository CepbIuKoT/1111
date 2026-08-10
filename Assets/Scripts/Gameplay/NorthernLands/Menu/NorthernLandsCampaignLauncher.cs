using Unity.BossRoom.Gameplay.NorthernLands.Content;
using Unity.BossRoom.Gameplay.NorthernLands.GameState;
using Unity.BossRoom.Gameplay.NorthernLands.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.BossRoom.Gameplay.NorthernLands.Menu
{
    /// <summary>
    /// Owns transitions from the main menu into the local campaign.
    /// </summary>
    public sealed class NorthernLandsCampaignLauncher
    {
        readonly NorthernLandsContentCatalog m_Content;
        readonly NorthernLandsProgressState m_Progress;
        readonly NorthernLandsSaveService m_Save;

        public bool CanContinue => m_Save.HasRunSave;

        public NorthernLandsCampaignLauncher(
            NorthernLandsContentCatalog content,
            NorthernLandsProgressState progress,
            NorthernLandsSaveService save)
        {
            m_Content = content;
            m_Progress = progress;
            m_Save = save;
        }

        public bool TryContinue(out string error)
        {
            if (!CanContinue)
            {
                error = "Сохранение пока не создано.";
                return false;
            }

            return TryLoadWorld(m_Progress.Run.currentWorld, out error);
        }

        public bool TryStartNewGame(out string error)
        {
            if (!m_Progress.HasPermanentRace)
            {
                error = "Сначала выберите постоянную расу.";
                return false;
            }

            var startWorld = m_Content.GetWorld(NorthernWorldId.NorthernLands);
            if (!Application.CanStreamedLevelBeLoaded(startWorld.sceneName))
            {
                error = "Риверхольм ещё не добавлен в текущую сборку.";
                return false;
            }

            m_Save.ResetRunKeepingRace();
            return TryLoadWorld(NorthernWorldId.NorthernLands, out error);
        }

        public bool TryChoosePermanentRace(string raceId, out string error)
        {
            if (!m_Progress.TryChoosePermanentRace(raceId, m_Content))
            {
                error = "Постоянная раса уже выбрана.";
                return false;
            }

            m_Save.SavePermanentRace();
            error = null;
            return true;
        }

        bool TryLoadWorld(NorthernWorldId worldId, out string error)
        {
            var world = m_Content.GetWorld(worldId);
            if (!Application.CanStreamedLevelBeLoaded(world.sceneName))
            {
                error = $"Локация «{world.displayName}» ещё не добавлена в текущую сборку.";
                return false;
            }

            m_Progress.EnterWorld(worldId);
            m_Save.SaveRun();
            error = null;
            SceneManager.LoadScene(world.sceneName);
            return true;
        }
    }
}