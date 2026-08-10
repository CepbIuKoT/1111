using UnityEngine;

namespace Unity.BossRoom.Gameplay.NorthernLands.Campaign
{
    /// <summary>
    /// Proximity endpoint for the first Riverholm quest giver.
    /// </summary>
    public sealed class NorthernLandsJarlNpc : MonoBehaviour
    {
        [SerializeField] float m_InteractionRange = 3.4f;

        public bool IsInRange(Transform player)
        {
            return player && Vector3.SqrMagnitude(player.position - transform.position) <= m_InteractionRange * m_InteractionRange;
        }
    }
}
