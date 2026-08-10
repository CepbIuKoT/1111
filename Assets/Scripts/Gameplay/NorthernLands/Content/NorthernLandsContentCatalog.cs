using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.NorthernLands.Content
{
    /// <summary>
    /// Loads the complete Northern Lands content manifest from Resources once per application lifetime.
    /// </summary>
    public sealed class NorthernLandsContentCatalog
    {
        public const int RequiredRaceCount = 45;
        public const int RequiredWorldCount = 7;
        const string k_ResourcePath = "NorthernLands/Data/northern_lands_content";

        readonly Dictionary<string, RaceDefinition> m_RacesById = new(StringComparer.Ordinal);
        readonly Dictionary<NorthernWorldId, WorldDefinition> m_WorldsById = new();

        public IReadOnlyCollection<RaceDefinition> Races => m_RacesById.Values;
        public IReadOnlyCollection<WorldDefinition> Worlds => m_WorldsById.Values;

        public NorthernLandsContentCatalog()
        {
            var asset = Resources.Load<TextAsset>(k_ResourcePath);
            if (!asset)
            {
                throw new InvalidOperationException($"Missing Northern Lands content manifest at Resources/{k_ResourcePath}.json");
            }

            var document = JsonUtility.FromJson<NorthernLandsContentDocument>(asset.text);
            ValidateAndIndex(document);
        }

        public RaceDefinition GetRace(string raceId)
        {
            if (string.IsNullOrWhiteSpace(raceId) || !m_RacesById.TryGetValue(raceId, out var race))
            {
                throw new KeyNotFoundException($"Unknown Northern Lands race '{raceId}'.");
            }

            return race;
        }

        public WorldDefinition GetWorld(NorthernWorldId worldId)
        {
            if (!m_WorldsById.TryGetValue(worldId, out var world))
            {
                throw new KeyNotFoundException($"Unknown Northern Lands world '{worldId}'.");
            }

            return world;
        }

        void ValidateAndIndex(NorthernLandsContentDocument document)
        {
            if (document == null || document.schemaVersion != 1)
            {
                throw new InvalidOperationException("Unsupported or empty Northern Lands content manifest.");
            }

            if (document.races == null || document.races.Length != RequiredRaceCount)
            {
                throw new InvalidOperationException($"Northern Lands requires exactly {RequiredRaceCount} races.");
            }

            if (document.worlds == null || document.worlds.Length != RequiredWorldCount)
            {
                throw new InvalidOperationException($"Northern Lands requires exactly {RequiredWorldCount} worlds.");
            }

            foreach (var race in document.races)
            {
                if (race == null || string.IsNullOrWhiteSpace(race.id) || !m_RacesById.TryAdd(race.id, race))
                {
                    throw new InvalidOperationException("Race identifiers must be present and unique.");
                }
            }

            foreach (var world in document.worlds)
            {
                if (world == null || string.IsNullOrWhiteSpace(world.sceneName) || !m_WorldsById.TryAdd(world.id, world))
                {
                    throw new InvalidOperationException("World identifiers and scene names must be present and unique.");
                }
            }
        }
    }
}
