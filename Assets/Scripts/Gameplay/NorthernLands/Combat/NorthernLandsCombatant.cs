using System.Collections;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.NorthernLands.Combat
{
    /// <summary>
    /// Lightweight local health/death state shared by the hero and campaign enemies.
    /// </summary>
    public sealed class NorthernLandsCombatant : MonoBehaviour
    {
        [SerializeField] float m_MaxHealth = 100f;
        [SerializeField] bool m_IsPlayer;

        Vector3 m_RespawnPoint;
        CharacterController m_Controller;
        Animator m_Animator;

        public float Health { get; private set; }
        public float MaxHealth => m_MaxHealth;
        public bool IsPlayer => m_IsPlayer;
        public bool IsAlive => Health > 0f;

        void Awake()
        {
            Health = m_MaxHealth;
            m_RespawnPoint = transform.position;
            m_Controller = GetComponent<CharacterController>();
            m_Animator = GetComponentInChildren<Animator>();
        }

        public void Configure(float maxHealth, bool isPlayer)
        {
            m_MaxHealth = Mathf.Max(1f, maxHealth);
            m_IsPlayer = isPlayer;
            Health = m_MaxHealth;
            m_RespawnPoint = transform.position;
        }

        public void ApplyDamage(float amount, Vector3 hitDirection)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            Health = Mathf.Max(0f, Health - amount);
            if (Health > 0f)
            {
                m_Animator?.SetTrigger("HitReact1");
                transform.position += Vector3.ProjectOnPlane(hitDirection, Vector3.up).normalized * 0.16f;
                return;
            }

            m_Animator?.SetTrigger("Dead");
            if (m_IsPlayer)
            {
                StartCoroutine(Respawn());
            }
            else
            {
                GetComponent<NorthernLandsEnemyAI>()?.OnDefeated();
                NorthernLandsLootPickup.Create(transform.position + Vector3.up * 0.7f);
                Destroy(gameObject, 4f);
            }
        }

        IEnumerator Respawn()
        {
            var motor = GetComponent<Unity.BossRoom.Gameplay.NorthernLands.Player.NorthernLandsThirdPersonMotor>();
            if (motor)
            {
                motor.enabled = false;
            }

            yield return new WaitForSeconds(2.5f);
            if (m_Controller)
            {
                m_Controller.enabled = false;
            }

            transform.position = m_RespawnPoint;
            if (m_Controller)
            {
                m_Controller.enabled = true;
            }

            Health = m_MaxHealth;
            m_Animator?.SetTrigger("BeginRevive");
            m_Animator?.SetTrigger("StandUp");
            if (motor)
            {
                motor.enabled = true;
            }
        }
    }
}
