using System.Collections;
using UnityEngine;

namespace NorthernLands.Combat
{
    [RequireComponent(typeof(HealthComponent))]
    public sealed class PlayerRespawnController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float respawnDelay = 2f;
        [SerializeField] private Vector3 spawnPosition;

        private HealthComponent _health;
        private CharacterController _controller;

        public void Configure(Vector3 position) => spawnPosition = position;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
            _controller = GetComponent<CharacterController>();
            _health.Died += OnDied;
        }

        private void OnDied(DamageInfo _) => StartCoroutine(RespawnAfterDelay());

        private IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(respawnDelay);
            if (_controller != null)
                _controller.enabled = false;

            transform.position = spawnPosition;
            if (_controller != null)
                _controller.enabled = true;

            _health.RestoreFull();
        }

        private void OnDestroy()
        {
            if (_health != null)
                _health.Died -= OnDied;
        }
    }
}
