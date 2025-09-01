using UnityEngine;
using System.Collections;

public class ManaOrb : MonoBehaviour
{
    [SerializeField] private float respawnTime = 10f;
    [SerializeField] private GameObject orbVisual; // child visual (sprite / particle)

    private Collider2D orbCollider;

    private void Awake()
    {
        orbCollider = GetComponent<Collider2D>();

        if (orbCollider == null)
        {
            orbCollider = gameObject.AddComponent<CircleCollider2D>();
            orbCollider.isTrigger = true;
        }

        if (orbVisual == null)
            orbVisual = this.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (ResourceManager.Instance != null)
            {
                // 🔥 Cek apakah mana sudah penuh
                if (ResourceManager.Instance.CurrentMana >= 2)
                {
                    Debug.Log("⚠️ Mana sudah penuh, orb tidak bisa diambil.");
                    return; // orb tetap ada, tidak hilang
                }

                // Tambahkan mana
                ResourceManager.Instance.AddMana(1);

                // Disable orb (visual + collider)
                StartCoroutine(RespawnRoutine());
            }
        }
    }

    private IEnumerator RespawnRoutine()
    {
        // Hide orb
        if (orbVisual != null) orbVisual.SetActive(false);
        if (orbCollider != null) orbCollider.enabled = false;

        // Wait
        yield return new WaitForSeconds(respawnTime);

        // Respawn orb
        if (orbVisual != null) orbVisual.SetActive(true);
        if (orbCollider != null) orbCollider.enabled = true;
    }
}
