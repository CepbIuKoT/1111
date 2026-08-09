using NorthernLands.Player.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NorthernLands.Combat
{
    public sealed class PlayerCombatController : MonoBehaviour
    {
        private readonly Collider[] _hits = new Collider[24];
        private static readonly float[] ComboDamage = { 16f, 21f, 29f };

        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private HealthComponent health;
        [SerializeField] private Transform attackOrigin;
        [SerializeField, Min(0.2f)] private float attackRadius = 1.35f;
        [SerializeField, Min(0.1f)] private float lightCooldown = 0.38f;
        [SerializeField, Min(0.1f)] private float heavyCooldown = 0.8f;
        [SerializeField, Min(0.1f)] private float comboResetSeconds = 1.1f;
        [SerializeField] private LayerMask hittableLayers = ~0;

        private float _nextAttackAt;
        private float _lastLightAt = float.NegativeInfinity;
        private int _comboIndex;

        public bool IsBlocking { get; private set; }
        public int ComboStep => _comboIndex;

        public void Configure(PlayerInputRouter inputRouter, HealthComponent healthComponent, Transform origin)
        {
            input = inputRouter;
            health = healthComponent;
            attackOrigin = origin;
        }

        private void Update()
        {
            if (health == null || health.IsDead)
                return;

            IsBlocking = ReadBlock();
            health.IsBlocking = IsBlocking;
            if (IsBlocking || Time.time < _nextAttackAt)
                return;

            if (ReadHeavyAttack())
            {
                PerformAttack(45f, heavyCooldown);
                _comboIndex = 0;
                return;
            }

            if (!ReadLightAttack())
                return;

            if (Time.time - _lastLightAt > comboResetSeconds)
                _comboIndex = 0;

            PerformAttack(ComboDamage[_comboIndex], lightCooldown);
            _lastLightAt = Time.time;
            _comboIndex = (_comboIndex + 1) % ComboDamage.Length;
        }

        private void PerformAttack(float damage, float cooldown)
        {
            _nextAttackAt = Time.time + cooldown;
            var origin = attackOrigin == null ? transform.position + transform.forward : attackOrigin.position;
            var count = Physics.OverlapSphereNonAlloc(
                origin,
                attackRadius,
                _hits,
                hittableLayers,
                QueryTriggerInteraction.Ignore);

            for (var index = 0; index < count; index++)
            {
                var hit = _hits[index];
                if (hit == null || hit.transform.root == transform.root)
                    continue;

                var damageable = hit.GetComponentInParent<IDamageable>();
                damageable?.TakeDamage(new DamageInfo(damage, gameObject));
            }
        }

        private bool ReadLightAttack()
            => (input != null && input.LightAttackPressed)
               || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        private bool ReadHeavyAttack()
            => (input != null && input.HeavyAttackPressed)
               || (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame);

        private bool ReadBlock()
            => (input != null && input.BlockHeld)
               || (Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.25f, 0.1f, 0.7f);
            var origin = attackOrigin == null ? transform.position + transform.forward : attackOrigin.position;
            Gizmos.DrawWireSphere(origin, attackRadius);
        }
    }
}
