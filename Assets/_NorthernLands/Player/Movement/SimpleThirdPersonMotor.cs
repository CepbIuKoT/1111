using NorthernLands.Combat;
using NorthernLands.Player.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NorthernLands.Player.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SimpleThirdPersonMotor : MonoBehaviour
    {
        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private HealthComponent health;
        [SerializeField, Min(0.1f)] private float movementSpeed = 5.2f;
        [SerializeField, Min(0.1f)] private float rotationSpeed = 14f;
        [SerializeField] private float gravity = -24f;

        private CharacterController _controller;
        private float _verticalVelocity;

        public void Configure(PlayerInputRouter inputRouter, Transform movementCamera, HealthComponent healthComponent)
        {
            input = inputRouter;
            cameraTransform = movementCamera;
            health = healthComponent;
        }

        private void Awake() => _controller = GetComponent<CharacterController>();

        private void Update()
        {
            if (health != null && health.IsDead)
                return;

            var moveInput = input == null ? Vector2.zero : input.Move;
            if (moveInput.sqrMagnitude < 0.001f)
                moveInput = ReadKeyboardMovement();

            var reference = cameraTransform == null ? transform : cameraTransform;
            var forward = Vector3.ProjectOnPlane(reference.forward, Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(reference.right, Vector3.up).normalized;
            var desired = Vector3.ClampMagnitude(forward * moveInput.y + right * moveInput.x, 1f);

            if (desired.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(desired),
                    rotationSpeed * Time.deltaTime);
            }

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += gravity * Time.deltaTime;

            var velocity = desired * movementSpeed;
            velocity.y = _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }

        private static Vector2 ReadKeyboardMovement()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return Vector2.zero;

            var x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
            var y = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
            return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        }
    }
}
