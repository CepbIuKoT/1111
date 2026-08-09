using NorthernLands.Player.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NorthernLands.CameraSystem
{
    public sealed class SimpleThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private Vector3 targetOffset = new(0f, 1.5f, 0f);
        [SerializeField] private float distance = 5.5f;
        [SerializeField] private float sensitivity = 0.12f;
        [SerializeField] private float smoothSpeed = 14f;

        private float _yaw;
        private float _pitch = 18f;

        public void Configure(Transform followTarget, PlayerInputRouter inputRouter)
        {
            target = followTarget;
            input = inputRouter;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            var look = input == null ? Vector2.zero : input.Look;
            if (Mouse.current != null && Mouse.current.middleButton.isPressed)
                look += Mouse.current.delta.ReadValue();

            _yaw += look.x * sensitivity;
            _pitch = Mathf.Clamp(_pitch - look.y * sensitivity, -10f, 65f);

            var rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            var focus = target.position + targetOffset;
            var desired = focus - rotation * Vector3.forward * distance;
            transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(focus - transform.position, Vector3.up);
        }
    }
}
