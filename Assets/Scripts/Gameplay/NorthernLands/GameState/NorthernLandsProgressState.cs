using System;
using Unity.BossRoom.Gameplay.NorthernLands.Content;
using Unity.BossRoom.Gameplay.NorthernLands.Items;
using Unity.BossRoom.Gameplay.NorthernLands.Progression;

namespace Unity.BossRoom.Gameplay.NorthernLands.GameState
{
    [Serializable]
    public class CityReputationData
    {
        public NorthernWorldId world;
        public int reputation;
        public bool isCriminal;
    }

    [Serializable]
    public class NorthernLandsSaveData
    {
        public int schemaVersion = 2;
        public NorthernWorldId currentWorld = NorthernWorldId.NorthernLands;
        public int level = 1;
        public int experience;
        public int pendingTalentChoices;
        public int gold;
        public int northernSilver;
        public bool hasWorldPosition;
        public float positionX;
        public float positionY;
        public float positionZ;
        public int deadWorldDeaths;
        public int soulKills;
        public int soulAsh;
        public bool towerUnlocked;
        public bool towerCompleted;
        public int towerTrialKills;
        public NorthernLandsHeroStats heroStats = new();
        public string[] learnedTalentIds = Array.Empty<string>();
        public string[] inventoryItemIds = Array.Empty<string>();
        public EquipmentItemData[] inventory = Array.Empty<EquipmentItemData>();
        public string equippedWeaponId;
        public string equippedArmorId;
        public string equippedRingId;
        public QuestProgressData[] quests = Array.Empty<QuestProgressData>();
        public CityReputationData[] cityReputations = Array.Empty<CityReputationData>();
    }

    [Serializable]
    public class EternalRaceSaveData
    {
        public int schemaVersion = 1;
        public string raceId;
    }

    /// <summary>
    /// Authoritative local progression for the single-player Northern Lands campaign.
    /// </summary>
    public sealed class NorthernLandsProgressState
    {
        public NorthernLandsSaveData Run { get; private set; } = NewRun();
        public EternalRaceSaveData EternalRace { get; private set; } = new();

        public bool HasPermanentRace => !string.IsNullOrWhiteSpace(EternalRace.raceId);

        public bool TryChoosePermanentRace(string raceId, NorthernLandsContentCatalog catalog)
        {
            if (HasPermanentRace)
            {
                return false;
            }

            catalog.GetRace(raceId);
            EternalRace.raceId = raceId;
            return true;
        }

        public void Restore(NorthernLandsSaveData run, EternalRaceSaveData eternalRace)
        {
            Run = run ?? NewRun();
            EternalRace = eternalRace ?? new EternalRaceSaveData();

            Run.learnedTalentIds ??= Array.Empty<string>();
            Run.inventoryItemIds ??= Array.Empty<string>();
            Run.inventory ??= Array.Empty<EquipmentItemData>();
            Run.quests ??= Array.Empty<QuestProgressData>();
            Run.cityReputations ??= Array.Empty<CityReputationData>();
            Run.heroStats ??= new NorthernLandsHeroStats();
            Run.schemaVersion = 2;
        }

        public void RecordSoulKill(bool droppedAsh)
        {
            Run.soulKills++;
            if (droppedAsh)
            {
                Run.soulAsh++;
            }

            Run.towerUnlocked = Run.soulKills >= 5 && Run.soulAsh >= 2;
        }

        public bool CanEnter(NorthernWorldId destination, RaceAbilityKind? activeRaceAbility = null)
        {
            return destination switch
            {
                NorthernWorldId.TowerOfGods => Run.towerUnlocked,
                NorthernWorldId.QuietDimension => activeRaceAbility == RaceAbilityKind.QuietDimension,
                _ => true
            };
        }

        public void EnterWorld(NorthernWorldId destination)
        {
            Run.currentWorld = destination;
        }

        public void HandleLivingWorldDeath()
        {
            Run.gold = Math.Max(0, Run.gold - Math.Max(1, Run.gold / 10));
            Run.currentWorld = NorthernWorldId.DeadWorld;
        }

        public bool HandleDeadWorldDeath()
        {
            Run.deadWorldDeaths++;
            if (Run.deadWorldDeaths < 2)
            {
                return false;
            }

            Run = NewRun();
            return true;
        }

        public void CompleteTowerTrial()
        {
            Run.towerCompleted = true;
            Run.towerTrialKills = 8;
            Run.deadWorldDeaths = 0;
            Run.soulKills = 0;
            Run.soulAsh = 0;
            Run.towerUnlocked = false;
            Run.currentWorld = NorthernWorldId.NorthernLands;
        }

        static NorthernLandsSaveData NewRun()
        {
            return new NorthernLandsSaveData
            {
                cityReputations = new[]
                {
                    new CityReputationData { world = NorthernWorldId.NorthernLands },
                    new CityReputationData { world = NorthernWorldId.AshenWorld },
                    new CityReputationData { world = NorthernWorldId.StarWastes }
                }
            };
        }
    }
}
