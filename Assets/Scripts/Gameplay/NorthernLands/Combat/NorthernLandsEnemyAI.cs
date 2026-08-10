using UnityEngine;

namespace Unity.BossRoom.Gameplay.NorthernLands.Combat
{
    /// <summary>
    /// Local pursuit and melee behaviour for the first Riverholm enemies.
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(NorthernLandsCombatant))]
    public sealed class NorthernLandsEnemyAI : MonoBehaviour
    {
        const float k_AggroRange = 22f;
        const float k_AttackRange = 2.15f;

        CharacterController m_Controller;
        NorthernLandsCombatant m_Combatant;
        NorthernLandsCombatant m_Player;
        Animator m_Animator;
        float m_NextSearch;
        float m_NextAttack;

        void Awake()
        {
            m_Controller = GetComponent<CharacterController>();
            m_Combatant = GetComponent<NorthernLandsCombatant>();
            m_Animator = GetComponentInChildren<Animator>();
        }

        void Update()
        {
            if (!m_Combatant.IsAlive)
            {
                return;
            }

            if ((!m_Player || !m_Player.IsAlive) && Time.time >= m_NextSearch)
            {
                m_NextSearch = Time.time + 0.75f;
                var combatants = FindObjectsByType<NorthernLandsCombatant>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var combatant in combatants)
                {
                    if (combatant.IsPlayer)
                    {
                        m_Player = combatant;
                        break;
                    }
                }
            }

            if (!m_Player || !m_Player.IsAlive)
            {
                SetSpeed(0f);
                return;
            }

            var offset = m_Player.transform.position - transform.position;
            offset.y = 0f;
            var distance = offset.magnitude;
            if (distance > k_AggroRange)
            {
                SetSpeed(0f);
                return;
            }

            if (offset.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(offset), 9f * Time.deltaTime);
            }

            if (distance > k_AttackRange)
            {
                m_Controller.SimpleMove(offset.normalized * 3.15f);
                SetSpeed(0.65f);
                return;
            }

            SetSpeed(0f);
            if (Time.time < m_NextAttack)
            {
                return;
            }

            m_NextAttack = Time.time + 1.35f;
            m_Animator?.SetTrigger("Attack1");
            m_Player.ApplyDamage(12f, offset.normalized);
        }

        public void OnDefeated()
        {
            enabled = false;
            if (m_Controller)
            {
                m_Controller.enabled = false;
            }
            SetSpeed(0f);
        }

        void SetSpeed(float value)
        {
            m_Animator?.SetFloat("Speed", value, 0.12f, Time.deltaTime);
        }
    }
}
