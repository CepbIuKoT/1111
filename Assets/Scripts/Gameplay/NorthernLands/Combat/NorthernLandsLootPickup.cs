using UnityEngine;

namespace Unity.BossRoom.Gameplay.NorthernLands.Combat
{
    /// <summary>
    /// Visible local loot drop used by the first combat loop.
    /// </summary>
    public sealed class NorthernLandsLootPickup : MonoBehaviour
    {
        const string k_SilverKey = "northern-lands-silver";
        Transform m_Player;

        public static int Silver => PlayerPrefs.GetInt(k_SilverKey, 0);

        public static void Create(Vector3 position)
        {
            var loot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            loot.name = "Northern Silver Loot";
            loot.transform.position = position;
            loot.transform.localScale = new Vector3(0.42f, 0.12f, 0.42f);
            var renderer = loot.GetComponent<Renderer>();
            renderer.material.color = new Color(0.25f, 0.75f, 1f);
            Object.Destroy(loot.GetComponent<Collider>());
            loot.AddComponent<NorthernLandsLootPickup>();
        }

        void Update()
        {
            transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
            transform.position += Vector3.up * Mathf.Sin(Time.time * 3f) * 0.0018f;
            if (!m_Player)
            {
                var combatants = FindObjectsByType<NorthernLandsCombatant>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var combatant in combatants)
                {
                    if (combatant.IsPlayer)
                    {
                        m_Player = combatant.transform;
                        break;
                    }
                }
            }

            if (m_Player && Vector3.SqrMagnitude(m_Player.position - transform.position) < 3.2f)
            {
                PlayerPrefs.SetInt(k_SilverKey, Silver + 1);
                PlayerPrefs.Save();
                Destroy(gameObject);
            }
        }
    }
}
