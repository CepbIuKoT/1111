using NorthernLands.Combat;
using NorthernLands.Player.Movement;
using UnityEngine;
using UnityEngine.UI;

namespace NorthernLands.UI.HUD
{
    public sealed class CombatSandboxHud : MonoBehaviour
    {
        [SerializeField] private HealthComponent playerHealth;
        [SerializeField] private DashChargeController dash;
        [SerializeField] private Text statusText;

        public void Configure(HealthComponent health, DashChargeController dashController, Text label)
        {
            playerHealth = health;
            dash = dashController;
            statusText = label;
        }

        private void Update()
        {
            if (playerHealth == null || statusText == null)
                return;

            statusText.text =
                $"Здоровье: {Mathf.CeilToInt(playerHealth.Current)}/{Mathf.CeilToInt(playerHealth.Maximum)}\n" +
                $"Рывки: {(dash == null ? 0 : dash.Charges)}/{(dash == null ? 0 : dash.MaximumCharges)}\n" +
                "ПК: WASD — ходьба, ЛКМ — атака, ПКМ — тяжёлая, Ctrl — блок, Space — рывок";
        }
    }
}
