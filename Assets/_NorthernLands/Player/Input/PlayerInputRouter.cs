using UnityEngine;
using UnityEngine.InputSystem;

namespace NorthernLands.Player.Input
{
    /// <summary>
    /// Shared input surface for keyboard/gamepad and mobile UI. Bind these references
    /// to the imported Starter Assets input-action asset instead of editing its code.
    /// </summary>
    public sealed class PlayerInputRouter : MonoBehaviour
    {
        private Vector2 _virtualMove;
        private Vector2 _virtualLook;
        private bool _virtualLightAttack;
        private bool _virtualHeavyAttack;
        private bool _virtualDash;
        private bool _virtualInteract;
        private bool _virtualRaceAbility;
        private bool _virtualBlock;

        [Header("Movement")]
        [SerializeField] private InputActionReference move;
        [SerializeField] private InputActionReference look;
        [SerializeField] private InputActionReference jump;
        [SerializeField] private InputActionReference sprint;

        [Header("Action RPG")]
        [SerializeField] private InputActionReference lightAttack;
        [SerializeField] private InputActionReference heavyAttack;
        [SerializeField] private InputActionReference block;
        [SerializeField] private InputActionReference dash;
        [SerializeField] private InputActionReference interact;
        [SerializeField] private InputActionReference raceAbility;

        public Vector2 Move => Vector2.ClampMagnitude(ReadVector2(move) + _virtualMove, 1f);
        public Vector2 Look
        {
            get
            {
                var value = ReadVector2(look) + _virtualLook;
                _virtualLook = Vector2.zero;
                return value;
            }
        }
        public bool JumpPressed => WasPressed(jump);
        public bool SprintHeld => IsPressed(sprint);
        public bool LightAttackPressed => WasPressed(lightAttack) || Consume(ref _virtualLightAttack);
        public bool HeavyAttackPressed => WasPressed(heavyAttack) || Consume(ref _virtualHeavyAttack);
        public bool BlockHeld => IsPressed(block) || _virtualBlock;
        public bool DashPressed => WasPressed(dash) || Consume(ref _virtualDash);
        public bool InteractPressed => WasPressed(interact) || Consume(ref _virtualInteract);
        public bool RaceAbilityPressed => WasPressed(raceAbility) || Consume(ref _virtualRaceAbility);

        public void SetVirtualMove(Vector2 value) => _virtualMove = Vector2.ClampMagnitude(value, 1f);
        public void AddVirtualLook(Vector2 delta) => _virtualLook += delta;
        public void SetVirtualBlock(bool held) => _virtualBlock = held;

        public void PressVirtualLightAttack() => _virtualLightAttack = true;
        public void PressVirtualHeavyAttack() => _virtualHeavyAttack = true;
        public void PressVirtualDash() => _virtualDash = true;
        public void PressVirtualInteract() => _virtualInteract = true;
        public void PressVirtualRaceAbility() => _virtualRaceAbility = true;

        private void OnEnable() => SetActionsEnabled(true);
        private void OnDisable() => SetActionsEnabled(false);

        private void SetActionsEnabled(bool enabled)
        {
            var references = new[]
            {
                move, look, jump, sprint, lightAttack, heavyAttack,
                block, dash, interact, raceAbility
            };

            foreach (var reference in references)
            {
                if (reference == null)
                    continue;

                if (enabled)
                    reference.action.Enable();
                else
                    reference.action.Disable();
            }
        }

        private static Vector2 ReadVector2(InputActionReference reference)
            => reference == null ? Vector2.zero : reference.action.ReadValue<Vector2>();

        private static bool WasPressed(InputActionReference reference)
            => reference != null && reference.action.WasPressedThisFrame();

        private static bool IsPressed(InputActionReference reference)
            => reference != null && reference.action.IsPressed();

        private static bool Consume(ref bool value)
        {
            if (!value)
                return false;

            value = false;
            return true;
        }
    }
}
