using Unity.BossRoom.Gameplay.NorthernLands.Content;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.NorthernLands.Campaign
{
    /// <summary>
    /// A world transition endpoint whose quest lock is controlled by the campaign director.
    /// </summary>
    public sealed class NorthernLandsWorldPortal : MonoBehaviour
    {
        [SerializeField] NorthernWorldId m_Destination = NorthernWorldId.DeadWorld;
        [SerializeField] float m_InteractionRange = 4.6f;
        [SerializeField] bool m_Unlocked;

        Renderer[] m_Renderers;

        public NorthernWorldId Destination => m_Destination;
        public bool Unlocked => m_Unlocked;

        void Awake()
        {
            m_Renderers = GetComponentsInChildren<Renderer>(true);
            RefreshVisuals();
        }

        public void Configure(NorthernWorldId destination, bool unlocked)
        {
            m_Destination = destination;
            m_Unlocked = unlocked;
            RefreshVisuals();
        }

        public void SetUnlocked(bool unlocked)
        {
            if (m_Unlocked == unlocked)
            {
                return;
            }

            m_Unlocked = unlocked;
            RefreshVisuals();
        }

        public bool IsInRange(Transform player)
        {
            return player && Vector3.SqrMagnitude(player.position - transform.position) <= m_InteractionRange * m_InteractionRange;
        }

        void RefreshVisuals()
        {
            if (m_Renderers == null)
            {
                return;
            }

            var color = m_Unlocked ? new Color(0.23f, 0.68f, 0.95f) : new Color(0.16f, 0.18f, 0.22f);
            foreach (var item in m_Renderers)
            {
                if (item && item.sharedMaterial)
                {
                    item.material.color = color;
                }
            }
        }
    }
}
