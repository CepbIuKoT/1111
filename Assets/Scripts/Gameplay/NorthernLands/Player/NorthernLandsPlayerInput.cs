using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.BossRoom.Gameplay.NorthernLands.Player
{
    /// <summary>
    /// Shared input state for desktop controls and the runtime Android HUD.
    /// </summary>
    public sealed class NorthernLandsPlayerInput : MonoBehaviour
    {
        Vector2 m_MobileMove;
        bool m_MobileSprint;
        bool m_AttackQueued;
        bool m_HeavyQueued;
        bool m_DodgeQueued;

        public Vector2 Move
        {
            get
            {
                var keyboard = Keyboard.current;
                if (keyboard == null)
                {
                    return m_MobileMove;
                }

                var desktop = Vector2.zero;
                desktop.x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
                desktop.y = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
                return desktop.sqrMagnitude > 0.01f ? Vector2.ClampMagnitude(desktop, 1f) : m_MobileMove;
            }
        }

        public bool SprintHeld => m_MobileSprint || (Keyboard.current?.leftShiftKey.isPressed ?? false);

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            m_AttackQueued |= keyboard?.fKey.wasPressedThisFrame ?? false;
            m_AttackQueued |= mouse?.leftButton.wasPressedThisFrame ?? false;
            m_HeavyQueued |= keyboard?.gKey.wasPressedThisFrame ?? false;
            m_HeavyQueued |= mouse?.rightButton.wasPressedThisFrame ?? false;
            m_DodgeQueued |= keyboard?.spaceKey.wasPressedThisFrame ?? false;
        }

        public void SetMobileMove(Vector2 value)
        {
            m_MobileMove = Vector2.ClampMagnitude(value, 1f);
        }

        public void SetMobileSprint(bool value)
        {
            m_MobileSprint = value;
        }

        public void QueueAttack()
        {
            m_AttackQueued = true;
        }

        public void QueueHeavyAttack()
        {
            m_HeavyQueued = true;
        }

        public void QueueDodge()
        {
            m_DodgeQueued = true;
        }

        public bool ConsumeAttack()
        {
            return Consume(ref m_AttackQueued);
        }

        public bool ConsumeHeavyAttack()
        {
            return Consume(ref m_HeavyQueued);
        }

        public bool ConsumeDodge()
        {
            return Consume(ref m_DodgeQueued);
        }

        static bool Consume(ref bool queued)
        {
            var value = queued;
            queued = false;
            return value;
        }
    }
}
