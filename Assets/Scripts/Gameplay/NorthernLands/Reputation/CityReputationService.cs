using System;
using Unity.BossRoom.Gameplay.NorthernLands.Content;
using Unity.BossRoom.Gameplay.NorthernLands.GameState;

namespace Unity.BossRoom.Gameplay.NorthernLands.Reputation
{
    public sealed class CityReputationService
    {
        readonly NorthernLandsProgressState m_Progress;

        public CityReputationService(NorthernLandsProgressState progress)
        {
            m_Progress = progress;
        }

        public CityReputationData Get(NorthernWorldId world)
        {
            var entries = m_Progress.Run.cityReputations;
            for (var i = 0; i < entries.Length; i++)
            {
                if (entries[i].world == world)
                {
                    return entries[i];
                }
            }

            var created = new CityReputationData { world = world };
            Array.Resize(ref entries, entries.Length + 1);
            entries[^1] = created;
            m_Progress.Run.cityReputations = entries;
            return created;
        }

        public void RecordCrime(NorthernWorldId world, int severity)
        {
            var city = Get(world);
            city.reputation -= Math.Max(1, severity);
            city.isCriminal = true;
        }

        public bool TryClearNameWithGold(NorthernWorldId world, int price)
        {
            var city = Get(world);
            if (!city.isCriminal || price < 0 || m_Progress.Run.gold < price)
            {
                return false;
            }

            m_Progress.Run.gold -= price;
            city.isCriminal = false;
            city.reputation = Math.Max(-10, city.reputation);
            return true;
        }

        public void ClearNameByExileContract(NorthernWorldId world)
        {
            var city = Get(world);
            city.isCriminal = false;
            city.reputation += 5;
        }
    }
}
