using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.BossRoom.Gameplay.NorthernLands.Player
{
    /// <summary>
    /// Collision-aware third-person orbit camera with mouse and right-side touch look.
    /// </summary>
    public sealed class NorthernLandsOrbitCamera : MonoBehaviour
    {
        [SerializeField] Transform m_Target;
        [SerializeField] float m_Distance = 6.3f;
        [SerializeField] float m_Height = 2.1f;

        float m_Yaw = 25f;
        float m_Pitch = 17f;
        int m_LookFinger = -1;
        Vector2 m_LastTouch;

        public void SetTarget(Transform target)
        {
            m_Target = target;
        }

        void LateUpdate()
        {
            if (!m_Target)
            {
                return;
            }

            ReadLookInput();
            var pivot = m_Target.position + Vector3.up * m_Height;
            var rotation = Quaternion.Euler(m_Pitch, m_Yaw, 0f);
            var desired = pivot - rotation * Vector3.forward * m_Distance;
            var direction = desired - pivot;
            var distance = direction.magnitude;

            if (Physics.SphereCast(pivot, 0.28f, direction.normalized, out var hit, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                desired = pivot + direction.normalized * Mathf.Max(0.7f, hit.distance - 0.2f);
            }

            transform.SetPositionAndRotation(
                Vector3.Lerp(transform.position, desired, 15f * Time.deltaTime),
                rotation);
        }

        void ReadLookInput()
        {
            var mouse = Mouse.current;
            if (mouse?.rightButton.isPressed ?? false)
            {
                var delta = mouse.delta.ReadValue();
                ApplyLook(delta * 0.14f);
            }

            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                return;
            }

            foreach (var touch in touchscreen.touches)
            {
                var id = touch.touchId.ReadValue();
                var pressed = touch.press.isPressed;
                var position = touch.position.ReadValue();
                if (m_LookFinger < 0 && pressed && position.x > Screen.width * 0.46f)
                {
                    m_LookFinger = id;
                    m_LastTouch = position;
                }

                if (id != m_LookFinger)
                {
                    continue;
                }

                if (!pressed)
                {
                    m_LookFinger = -1;
                    continue;
                }

                var delta = position - m_LastTouch;
                m_LastTouch = position;
                ApplyLook(delta * 0.09f);
            }
        }

        void ApplyLook(Vector2 delta)
        {
            m_Yaw += delta.x;
            m_Pitch = Mathf.Clamp(m_Pitch - delta.y, -8f, 58f);
        }
    }
}
