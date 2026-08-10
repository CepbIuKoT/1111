using System;
using Unity.BossRoom.Gameplay.NorthernLands.GameState;
using Unity.BossRoom.Gameplay.NorthernLands.Content;
using Unity.BossRoom.Gameplay.NorthernLands.Persistence;
using Unity.BossRoom.Gameplay.NorthernLands.Progression;
using Unity.BossRoom.Gameplay.NorthernLands.Quests;
using Unity.BossRoom.Gameplay.NorthernLands.World;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Unity.BossRoom.Gameplay.NorthernLands.Campaign
{
    /// <summary>
    /// Connects generated campaign scenes to application-lifetime services.
    /// </summary>
    public sealed class NorthernLandsCampaignRuntime : IStartable, IDisposable
    {
        readonly QuestProgressService m_Quests;
        readonly HeroProgressionService m_Progression;
        readonly NorthernLandsProgressState m_State;
        readonly NorthernLandsSaveService m_Save;
        readonly NorthernLandsWorldTravelService m_Travel;
        readonly NorthernLandsContentCatalog m_Content;

        public NorthernLandsCampaignRuntime(
            QuestProgressService quests,
            HeroProgressionService progression,
            NorthernLandsProgressState state,
            NorthernLandsSaveService save,
            NorthernLandsWorldTravelService travel,
            NorthernLandsContentCatalog content)
        {
            m_Quests = quests;
            m_Progression = progression;
            m_State = state;
            m_Save = save;
            m_Travel = travel;
            m_Content = content;
        }

        public void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitializeActiveScene();
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InitializeActiveScene();
        }

        void InitializeActiveScene()
        {
            var director = UnityEngine.Object.FindFirstObjectByType<NorthernLandsCampaignDirector>();
            if (director)
            {
                director.Initialize(m_Quests, m_Progression, m_State, m_Save, m_Travel, m_Content);
            }
        }
    }
}
