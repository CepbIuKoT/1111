using UnityEngine;

namespace Unity.BossRoom.Gameplay.NorthernLands.Combat
{
    /// <summary>
    /// Resolves close-range hero attacks against local enemies without allocations.
    /// </summary>
    [RequireComponent(typeof(NorthernLandsCombatant))]
    public sealed class NorthernLandsPlayerCombat : MonoBehaviour
    {
        readonly Collider[] m_Hits = new Collider[20];
        float m_NextAttackTime;

        public bool TryAttack(bool heavy)
        {
            if (Time.time < m_NextAttackTime)
            {
                return false;
            }

            m_NextAttackTime = Time.time + (heavy ? 0.95f : 0.52f);
            var center = transform.position + Vector3.up * 1.05f + transform.forward * (heavy ? 1.55f : 1.25f);
            var radius = heavy ? 1.65f : 1.25f;
            var count = Physics.OverlapSphereNonAlloc(center, radius, m_Hits, ~0, QueryTriggerInteraction.Ignore);
            var damage = heavy ? 44f : 25f;
            for (var i = 0; i < count; i++)
            {
                var target = m_Hits[i].GetComponentInParent<NorthernLandsCombatant>();
                if (!target || target.IsPlayer || !target.IsAlive)
                {
                    continue;
                }

                target.ApplyDamage(damage, transform.forward);
            }

            return true;
        }
    }
}
