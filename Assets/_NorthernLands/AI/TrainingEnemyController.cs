using NorthernLands.Combat;
using UnityEngine;

namespace NorthernLands.AI
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class TrainingEnemyController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0.1f)] private float movementSpeed = 2.2f;
        [SerializeField, Min(0.1f)] private float detectionDistance = 12f;
        [SerializeField, Min(0.1f)] private float attackDistance = 1.6f;
        [SerializeField, Min(0.1f)] private float attackCooldown = 1.3f;
        [SerializeField, Min(0f)] private float attackDamage = 13f;

        private CharacterController _controller;
        private HealthComponent _health;
        private float _nextAttackAt;

        public void Configure(Transform attackTarget) => target = attackTarget;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _health = GetComponent<HealthComponent>();
            _health.Died += OnDied;
        }

        private void Update()
        {
            if (_health.IsDead || target == null)
                return;

            var delta = target.position - transform.position;
            delta.y = 0f;
            var distance = delta.magnitude;
            if (distance > detectionDistance)
                return;

            if (distance > attackDistance)
            {
                var direction = delta.normalized;
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    9f * Time.deltaTime);
                _controller.Move(direction * movementSpeed * Time.deltaTime);
                return;
            }

            if (Time.time < _nextAttackAt)
                return;

            _nextAttackAt = Time.time + attackCooldown;
            var damageable = target.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(new DamageInfo(attackDamage, gameObject));
        }

        private void OnDied(DamageInfo _)
        {
            _controller.enabled = false;
            transform.localScale = new Vector3(1f, 0.25f, 1f);
            enabled = false;
        }

        private void OnDestroy()
        {
            if (_health != null)
                _health.Died -= OnDied;
        }
    }
}
