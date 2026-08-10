using UnityEngine;

namespace Unity.BossRoom.Gameplay.NorthernLands.Player
{
    /// <summary>
    /// Local campaign locomotion. It deliberately has no Netcode dependency.
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(NorthernLandsPlayerInput))]
    public sealed class NorthernLandsThirdPersonMotor : MonoBehaviour
    {
        const float k_WalkSpeed = 4.4f;
        const float k_RunSpeed = 7.2f;
        const float k_Gravity = -24f;
        const float k_DodgeSpeed = 11f;
        const float k_DodgeDuration = 0.32f;

        CharacterController m_Controller;
        NorthernLandsPlayerInput m_Input;
        Animator m_Animator;
        Transform m_Camera;
        Vector3 m_Velocity;
        Vector3 m_DodgeDirection;
        float m_DodgeRemaining;

        void Awake()
        {
            m_Controller = GetComponent<CharacterController>();
            m_Input = GetComponent<NorthernLandsPlayerInput>();
            m_Animator = GetComponentInChildren<Animator>();
            m_Camera = Camera.main ? Camera.main.transform : null;
        }

        void Update()
        {
            m_Camera = m_Camera ? m_Camera : Camera.main?.transform;
            var input = m_Input.Move;
            var desiredDirection = CameraRelativeDirection(input);

            if (m_Input.ConsumeDodge() && m_DodgeRemaining <= 0f)
            {
                m_DodgeDirection = desiredDirection.sqrMagnitude > 0.01f ? desiredDirection : transform.forward;
                m_DodgeRemaining = k_DodgeDuration;
            }

            var planarVelocity = desiredDirection * (m_Input.SprintHeld ? k_RunSpeed : k_WalkSpeed);
            if (m_DodgeRemaining > 0f)
            {
                m_DodgeRemaining -= Time.deltaTime;
                planarVelocity = m_DodgeDirection * k_DodgeSpeed;
            }

            if (desiredDirection.sqrMagnitude > 0.01f)
            {
                var targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 14f * Time.deltaTime);
            }

            m_Velocity.x = planarVelocity.x;
            m_Velocity.z = planarVelocity.z;
            m_Velocity.y = m_Controller.isGrounded ? -2f : m_Velocity.y + k_Gravity * Time.deltaTime;
            m_Controller.Move(m_Velocity * Time.deltaTime);

            if (m_Animator)
            {
                m_Animator.SetFloat("Speed", planarVelocity.magnitude / k_RunSpeed, 0.12f, Time.deltaTime);
                if (m_Input.ConsumeAttack())
                {
                    m_Animator.SetTrigger("Attack1");
                }

                if (m_Input.ConsumeHeavyAttack())
                {
                    m_Animator.SetTrigger("Attack2");
                }
            }
            else
            {
                m_Input.ConsumeAttack();
                m_Input.ConsumeHeavyAttack();
            }
        }

        Vector3 CameraRelativeDirection(Vector2 input)
        {
            if (input.sqrMagnitude < 0.01f)
            {
                return Vector3.zero;
            }

            var forward = m_Camera ? Vector3.ProjectOnPlane(m_Camera.forward, Vector3.up).normalized : Vector3.forward;
            var right = m_Camera ? Vector3.ProjectOnPlane(m_Camera.right, Vector3.up).normalized : Vector3.right;
            return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
        }
    }
}
