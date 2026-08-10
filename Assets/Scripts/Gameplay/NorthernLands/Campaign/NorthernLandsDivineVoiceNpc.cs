using UnityEngine;

namespace Unity.BossRoom.Gameplay.NorthernLands.Campaign
{
    /// <summary>
    /// Proximity endpoint for the Tower of Gods trial choice.
    /// </summary>
    public sealed class NorthernLandsDivineVoiceNpc : MonoBehaviour
    {
        [SerializeField] float m_InteractionRange = 4f;

        public bool IsInRange(Transform player)
        {
            return player && Vector3.SqrMagnitude(player.position - transform.position) <= m_InteractionRange * m_InteractionRange;
        }
    }
}
