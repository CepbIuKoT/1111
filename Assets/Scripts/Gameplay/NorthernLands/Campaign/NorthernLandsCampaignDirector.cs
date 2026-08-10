using System;
using System.Collections.Generic;
using Unity.BossRoom.Gameplay.NorthernLands.Combat;
using Unity.BossRoom.Gameplay.NorthernLands.Content;
using Unity.BossRoom.Gameplay.NorthernLands.GameState;
using Unity.BossRoom.Gameplay.NorthernLands.Persistence;
using Unity.BossRoom.Gameplay.NorthernLands.Progression;
using Unity.BossRoom.Gameplay.NorthernLands.Quests;
using Unity.BossRoom.Gameplay.NorthernLands.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Unity.BossRoom.Gameplay.NorthernLands.Campaign
{
    /// <summary>
    /// Owns the first Riverholm quest, autosave, NPC interaction and portal gate.
    /// </summary>
    public sealed class NorthernLandsCampaignDirector : MonoBehaviour
    {
        const float k_AutosaveInterval = 10f;
        const int k_RequiredImpKills = 4;
        const int k_RequiredTowerKills = 8;

        [SerializeField] Transform m_Player;
        [SerializeField] NorthernWorldId m_World = NorthernWorldId.NorthernLands;
        [SerializeField] NorthernLandsCombatant m_PlayerCombatant;
        [SerializeField] NorthernLandsJarlNpc m_Jarl;
        [SerializeField] NorthernLandsDivineVoiceNpc m_DivineVoice;
        [SerializeField] NorthernLandsWorldPortal m_Portal;
        [SerializeField] NorthernLandsWorldPortal m_ReturnPortal;

        readonly List<NorthernLandsCombatant> m_Enemies = new();

        QuestProgressService m_Quests;
        HeroProgressionService m_Progression;
        NorthernLandsProgressState m_State;
        NorthernLandsSaveService m_Save;
        NorthernLandsWorldTravelService m_Travel;
        NorthernLandsContentCatalog m_Content;
        float m_NextAutosave;
        string m_Status = "Доберитесь до ярла Ингвара в главном зале Риверхольма.";

        public event Action UiChanged;
        public bool IsInitialized { get; private set; }
        public bool TowerChoiceVisible { get; private set; }
        public int NorthernSilver => m_State?.Run.northernSilver ?? 0;
        public string StatusText => m_Status;
        public string LocationText => m_World switch
        {
            NorthernWorldId.DeadWorld => "МИР МЁРТВЫХ  •  БЕРЕГ ЗАБЫТЫХ",
            NorthernWorldId.TowerOfGods => "БАШНЯ БОГОВ  •  ЗАЛ ИСПЫТАНИЯ",
            _ => "СЕВЕРНЫЕ ЗЕМЛИ  •  РИВЕРХОЛЬМ"
        };

        public string ObjectiveText
        {
            get
            {
                if (!IsInitialized)
                {
                    return "Загрузка задания…";
                }

                if (m_World == NorthernWorldId.DeadWorld)
                {
                    return m_State.Run.towerUnlocked
                        ? "ЗАДАНИЕ: путь к Башне богов открыт"
                        : $"ЗАДАНИЕ: души {m_State.Run.soulKills}/5  •  пепел {m_State.Run.soulAsh}/2";
                }

                if (m_World == NorthernWorldId.TowerOfGods)
                {
                    if (!m_State.Run.towerTrialStarted)
                    {
                        return "ЗАДАНИЕ: поговорить с Гласом богов";
                    }

                    return m_State.Run.towerCompleted
                        ? "ЗАДАНИЕ: испытание завершено — войдите во Врата жизни"
                        : $"ЗАДАНИЕ: стражи башни {m_State.Run.towerTrialKills}/{k_RequiredTowerKills}";
                }

                var quest = m_Quests.Get(QuestProgressService.FirstHuntQuestId);
                if (quest == null)
                {
                    return "ЗАДАНИЕ: поговорить с ярлом Ингваром";
                }

                if (!quest.completed)
                {
                    return $"ЗАДАНИЕ: морозные бесы  {quest.currentAmount}/{quest.requiredAmount}";
                }

                return quest.rewardClaimed
                    ? "ЗАДАНИЕ: исследовать врата в Мир мёртвых"
                    : "ЗАДАНИЕ: вернуться к ярлу Ингвару";
            }
        }

        public string InteractionText
        {
            get
            {
                if (m_Jarl && m_Jarl.IsInRange(m_Player))
                {
                    return "ГОВОРИТЬ";
                }


                if (m_DivineVoice && m_DivineVoice.IsInRange(m_Player) && !m_State.Run.towerTrialStarted)
                {
                    return "СЛУШАТЬ ГЛАС";
                }

                if (m_Portal && m_Portal.IsInRange(m_Player))
                {
                    return m_Portal.Unlocked ? "ВОЙТИ" : "ВРАТА ЗАКРЫТЫ";
                }


                if (m_ReturnPortal && m_ReturnPortal.IsInRange(m_Player))
                {
                    return "ВЕРНУТЬСЯ";
                }

                return string.Empty;
            }
        }

        public Transform NavigationTarget
        {
            get
            {
                if (!IsInitialized || !m_Player)
                {
                    return null;
                }

                if (m_World == NorthernWorldId.DeadWorld)
                {
                    return m_State.Run.towerUnlocked ? m_Portal?.transform : ClosestLivingEnemy();
                }

                if (m_World == NorthernWorldId.TowerOfGods)
                {
                    if (!m_State.Run.towerTrialStarted)
                    {
                        return m_DivineVoice?.transform;
                    }

                    return m_State.Run.towerCompleted ? m_Portal?.transform : ClosestLivingEnemy();
                }

                var quest = m_Quests.Get(QuestProgressService.FirstHuntQuestId);
                if (quest == null || (quest.completed && !quest.rewardClaimed))
                {
                    return m_Jarl?.transform;
                }

                return quest.completed ? m_Portal?.transform : ClosestLivingEnemy();
            }
        }

        public string NavigationTargetText
        {
            get
            {
                if (m_World == NorthernWorldId.DeadWorld)
                {
                    return m_State.Run.towerUnlocked ? "ВХОД В БАШНЮ БОГОВ" : "БЛИЖАЙШАЯ ДУША";
                }

                if (m_World == NorthernWorldId.TowerOfGods)
                {
                    if (!m_State.Run.towerTrialStarted)
                    {
                        return "ГЛАС БОГОВ";
                    }
                    return m_State.Run.towerCompleted ? "ВРАТА ЖИЗНИ" : "БЛИЖАЙШИЙ СТРАЖ";
                }

                var quest = m_Quests.Get(QuestProgressService.FirstHuntQuestId);
                if (quest == null || (quest.completed && !quest.rewardClaimed))
                {
                    return "ЯРЛ ИНГВАР";
                }
                return quest.completed ? "ВРАТА В МИР МЁРТВЫХ" : "БЛИЖАЙШИЙ ВРАГ";
            }
        }

        public void Configure(
            NorthernWorldId world,
            Transform player,
            NorthernLandsCombatant playerCombatant,
            NorthernLandsJarlNpc jarl,
            NorthernLandsWorldPortal portal,
            NorthernLandsWorldPortal returnPortal = null,
            NorthernLandsDivineVoiceNpc divineVoice = null)
        {
            m_World = world;
            m_Player = player;
            m_PlayerCombatant = playerCombatant;
            m_Jarl = jarl;
            m_Portal = portal;
            m_ReturnPortal = returnPortal;
            m_DivineVoice = divineVoice;
        }

        public void Initialize(
            QuestProgressService quests,
            HeroProgressionService progression,
            NorthernLandsProgressState state,
            NorthernLandsSaveService save,
            NorthernLandsWorldTravelService travel,
            NorthernLandsContentCatalog content)
        {
            if (IsInitialized)
            {
                return;
            }

            m_Quests = quests;
            m_Progression = progression;
            m_State = state;
            m_Save = save;
            m_Travel = travel;
            m_Content = content;
            IsInitialized = true;

            m_Status = m_World switch
            {
                NorthernWorldId.DeadWorld => "Соберите силу павших душ и отыщите путь к Башне богов.",
                NorthernWorldId.TowerOfGods => "Найдите Глас богов и выберите путь испытания.",
                _ => "Доберитесь до ярла Ингвара в главном зале Риверхольма."
            };

            RestoreHero();
            if (m_PlayerCombatant)
            {
                m_PlayerCombatant.Defeated += OnPlayerDefeated;
            }
            SubscribeEnemies();
            RefreshPortal();
            m_NextAutosave = Time.unscaledTime + k_AutosaveInterval;
            NotifyUi();
        }

        void Update()
        {
            if (!IsInitialized)
            {
                return;
            }

            if (Keyboard.current?.eKey.wasPressedThisFrame ?? false)
            {
                TryInteract();
            }

            if (Time.unscaledTime >= m_NextAutosave)
            {
                m_NextAutosave = Time.unscaledTime + k_AutosaveInterval;
                SaveCurrentState();
            }
        }

        void OnDestroy()
        {
            if (TowerChoiceVisible)
            {
                Time.timeScale = 1f;
            }

            if (m_PlayerCombatant)
            {
                m_PlayerCombatant.Defeated -= OnPlayerDefeated;
            }

            foreach (var enemy in m_Enemies)
            {
                if (enemy)
                {
                    enemy.Defeated -= OnEnemyDefeated;
                }
            }
        }

        void OnApplicationPause(bool paused)
        {
            if (paused && IsInitialized)
            {
                SaveCurrentState();
            }
        }

        void OnApplicationQuit()
        {
            if (IsInitialized)
            {
                SaveCurrentState();
            }
        }

        public void TryInteract()
        {
            if (!IsInitialized)
            {
                return;
            }

            if (m_Jarl && m_Jarl.IsInRange(m_Player))
            {
                InteractWithJarl();
                return;
            }

            if (m_DivineVoice && m_DivineVoice.IsInRange(m_Player) && !m_State.Run.towerTrialStarted)
            {
                InteractWithDivineVoice();
                return;
            }

            if (m_Portal && m_Portal.IsInRange(m_Player))
            {
                InteractWithPortal();
                return;
            }

            if (m_ReturnPortal && m_ReturnPortal.IsInRange(m_Player))
            {
                InteractWithPortal(m_ReturnPortal);
            }
        }

        public void CollectNorthernSilver(int amount)
        {
            if (!IsInitialized || amount <= 0)
            {
                return;
            }

            m_State.Run.northernSilver += amount;
            m_Status = $"Получено северное серебро: +{amount}.";
            SaveCurrentState();
            NotifyUi();
        }

        void InteractWithJarl()
        {
            var quest = m_Quests.Get(QuestProgressService.FirstHuntQuestId);
            if (quest == null)
            {
                m_Quests.Start(QuestProgressService.FirstHuntQuestId, k_RequiredImpKills);
                m_Status = "Ярл Ингвар: морозные бесы перекрыли дороги. Уничтожьте четверых и возвращайтесь.";
            }
            else if (!quest.completed)
            {
                m_Status = $"Ярл Ингвар: дороги ещё опасны. Уничтожено бесов: {quest.currentAmount}/{quest.requiredAmount}.";
            }
            else if (!quest.rewardClaimed)
            {
                m_Quests.TryClaimReward(QuestProgressService.FirstHuntQuestId, 75, 120, m_Progression);
                m_Status = "Ярл Ингвар: Риверхольм в долгу перед вами. Врата на востоке теперь открыты.";
            }
            else
            {
                m_Status = "Ярл Ингвар: за вратами лежит Мир мёртвых. Возвращайтесь живым.";
            }

            RefreshPortal();
            SaveCurrentState();
            NotifyUi();
        }

        void InteractWithPortal(NorthernLandsWorldPortal selectedPortal = null)
        {
            var portal = selectedPortal ? selectedPortal : m_Portal;
            if (!portal || !portal.Unlocked)
            {
                m_Status = m_World switch
                {
                    NorthernWorldId.DeadWorld => "Путь к Башне богов запечатан. Соберите пять душ и два сгустка пепла.",
                    NorthernWorldId.TowerOfGods => "Врата жизни откроются после победы над всеми стражами.",
                    _ => "Древние врата запечатаны. Сначала помогите ярлу Риверхольма."
                };
                NotifyUi();
                return;
            }

            var world = portal.Destination;
            var sceneName = m_Content.GetWorld(world).sceneName;
            if (!m_State.CanEnter(world) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                m_Status = "Врата пробудились, но путь в следующий мир ещё не завершён.";
                NotifyUi();
                return;
            }

            SaveCurrentState();
            if (!m_Travel.TryTravel(world, null, out sceneName))
            {
                m_Status = "Переход сейчас недоступен.";
                NotifyUi();
                return;
            }
            SceneManager.LoadScene(sceneName);
        }

        void InteractWithDivineVoice()
        {
            m_Status = "Глас богов: что становится больше, чем больше у него отнимают? Выберите ответ или бой.";
            TowerChoiceVisible = true;
            Time.timeScale = 0f;
            NotifyUi();
        }

        public void ChooseTowerCombat()
        {
            if (!IsInitialized || m_World != NorthernWorldId.TowerOfGods || m_State.Run.towerCompleted)
            {
                return;
            }

            m_State.Run.towerTrialStarted = true;
            TowerChoiceVisible = false;
            Time.timeScale = 1f;
            m_Status = "Глас богов: докажите право на жизнь. Победите восьмерых стражей.";
            SaveCurrentState();
            NotifyUi();
        }

        public void ChooseTowerRiddle()
        {
            if (!IsInitialized || m_World != NorthernWorldId.TowerOfGods || m_State.Run.towerCompleted)
            {
                return;
            }

            m_State.Run.towerTrialStarted = true;
            m_State.Run.towerCompleted = true;
            TowerChoiceVisible = false;
            Time.timeScale = 1f;
            m_Status = "Верно: яма. Испытание пройдено, Врата жизни открыты.";
            DismissTowerGuardians();
            RefreshPortal();
            SaveCurrentState();
            NotifyUi();
        }

        void OnEnemyDefeated(NorthernLandsCombatant enemy)
        {
            if (m_World == NorthernWorldId.DeadWorld)
            {
                var droppedAsh = (m_State.Run.soulKills + 1) % 2 == 0;
                m_State.RecordSoulKill(droppedAsh);
                m_Status = m_State.Run.towerUnlocked
                    ? "Сила собрана. Вдали открылся путь к Башне богов."
                    : $"Поглощена душа: {m_State.Run.soulKills}/5. Пепел душ: {m_State.Run.soulAsh}/2.";
                RefreshPortal();
                SaveCurrentState();
                NotifyUi();
                return;
            }


            if (m_World == NorthernWorldId.TowerOfGods)
            {
                m_State.Run.towerTrialStarted = true;
                if (m_State.Run.towerCompleted)
                {
                    return;
                }

                m_State.Run.towerTrialKills = Math.Min(k_RequiredTowerKills, m_State.Run.towerTrialKills + 1);
                if (m_State.Run.towerTrialKills >= k_RequiredTowerKills)
                {
                    m_State.Run.towerCompleted = true;
                    m_Status = "Испытание пройдено. Врата жизни ведут обратно в Северные земли.";
                    RefreshPortal();
                }
                else
                {
                    m_Status = $"Страж башни повержен: {m_State.Run.towerTrialKills}/{k_RequiredTowerKills}.";
                }
                SaveCurrentState();
                NotifyUi();
                return;
            }

            var quest = m_Quests.Get(QuestProgressService.FirstHuntQuestId);
            if (quest == null || quest.completed)
            {
                return;
            }

            var completed = m_Quests.AddProgress(QuestProgressService.FirstHuntQuestId, 1);
            quest = m_Quests.Get(QuestProgressService.FirstHuntQuestId);
            m_Status = completed
                ? "Дороги очищены. Вернитесь к ярлу Ингвару."
                : $"Морозный бес повержен: {quest.currentAmount}/{quest.requiredAmount}.";
            SaveCurrentState();
            NotifyUi();
        }

        void DismissTowerGuardians()
        {
            foreach (var enemy in m_Enemies)
            {
                if (enemy)
                {
                    enemy.gameObject.SetActive(false);
                }
            }
        }

        void OnPlayerDefeated(NorthernLandsCombatant player)
        {
            if (m_World == NorthernWorldId.DeadWorld)
            {
                var runReset = m_State.HandleDeadWorldDeath();
                m_Save.SaveRun();
                if (!runReset)
                {
                    m_Status = "Душа раскололась. Ещё одна гибель полностью сбросит прохождение.";
                    NotifyUi();
                    return;
                }

                var livingScene = m_Content.GetWorld(NorthernWorldId.NorthernLands).sceneName;
                if (Application.CanStreamedLevelBeLoaded(livingScene))
                {
                    SceneManager.LoadScene(livingScene);
                }
                return;
            }

            m_State.HandleLivingWorldDeath();
            m_Save.SaveRun();
            var deadScene = m_Content.GetWorld(NorthernWorldId.DeadWorld).sceneName;
            if (Application.CanStreamedLevelBeLoaded(deadScene))
            {
                SceneManager.LoadScene(deadScene);
            }
        }

        void SubscribeEnemies()
        {
            var combatants = FindObjectsByType<NorthernLandsCombatant>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var combatant in combatants)
            {
                if (combatant.IsPlayer)
                {
                    continue;
                }

                combatant.Defeated += OnEnemyDefeated;
                m_Enemies.Add(combatant);
            }
        }

        void RestoreHero()
        {
            if (!m_Player || !m_PlayerCombatant)
            {
                return;
            }

            var run = m_State.Run;
            if (run.currentWorld == NorthernWorldId.NorthernLands && run.hasWorldPosition)
            {
                var controller = m_Player.GetComponent<CharacterController>();
                if (controller)
                {
                    controller.enabled = false;
                }

                m_Player.position = new Vector3(run.positionX, run.positionY, run.positionZ);
                if (controller)
                {
                    controller.enabled = true;
                }
            }

            m_PlayerCombatant.RestoreHealth(run.heroStats.currentHealth);
        }

        void SaveCurrentState()
        {
            if (!m_Player || !m_PlayerCombatant)
            {
                return;
            }

            var run = m_State.Run;
            run.currentWorld = m_World;
            if (m_World == NorthernWorldId.NorthernLands)
            {
                run.hasWorldPosition = true;
                run.positionX = m_Player.position.x;
                run.positionY = m_Player.position.y;
                run.positionZ = m_Player.position.z;
            }
            run.heroStats.currentHealth = m_PlayerCombatant.Health;
            m_Save.SaveRun();
        }

        void RefreshPortal()
        {
            if (!m_Portal)
            {
                return;
            }

            if (m_World == NorthernWorldId.DeadWorld)
            {
                m_Portal.SetUnlocked(m_State.Run.towerUnlocked);
                m_ReturnPortal?.SetUnlocked(true);
                return;
            }

            if (m_World == NorthernWorldId.TowerOfGods)
            {
                m_Portal.SetUnlocked(m_State.Run.towerCompleted);
                return;
            }

            var quest = m_Quests.Get(QuestProgressService.FirstHuntQuestId);
            m_Portal.SetUnlocked(quest?.rewardClaimed ?? false);
        }

        Transform ClosestLivingEnemy()
        {
            Transform closest = null;
            var closestDistance = float.MaxValue;
            foreach (var enemy in m_Enemies)
            {
                if (!enemy || !enemy.IsAlive)
                {
                    continue;
                }

                var distance = (enemy.transform.position - m_Player.position).sqrMagnitude;
                if (distance >= closestDistance)
                {
                    continue;
                }

                closestDistance = distance;
                closest = enemy.transform;
            }
            return closest;
        }

        void NotifyUi()
        {
            UiChanged?.Invoke();
        }
    }
}
