using NorthernLands.Combat;
using NorthernLands.Player.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NorthernLands.Player.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class DashChargeController : MonoBehaviour
    {
        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private HealthComponent health;
        [SerializeField, Range(1, 5)] private int maximumCharges = 2;
        [SerializeField, Min(0.1f)] private float rechargeSeconds = 2.5f;
        [SerializeField, Min(0.1f)] private float distance = 3.2f;

        private CharacterController _controller;
        private float _nextRechargeAt;

        public int Charges { get; private set; }
        public int MaximumCharges => maximumCharges;

        public void Configure(PlayerInputRouter inputRouter, HealthComponent healthComponent)
        {
            input = inputRouter;
            health = healthComponent;
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            Charges = maximumCharges;
        }

        private void Update()
        {
            Recharge();
            if (health != null && health.IsDead)
                return;

            var pressed = input != null && input.DashPressed;
            pressed |= Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
            if (!pressed || Charges <= 0)
                return;

            Charges--;
            if (Charges == maximumCharges - 1)
                _nextRechargeAt = Time.time + rechargeSeconds;

            _controller.Move(transform.forward * distance);
        }

        private void Recharge()
        {
            if (Charges >= maximumCharges || Time.time < _nextRechargeAt)
                return;

            Charges++;
            _nextRechargeAt = Time.time + rechargeSeconds;
        }
    }
}
