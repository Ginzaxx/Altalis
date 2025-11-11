using System.Collections;
using UnityEngine;

public class ManaBlock : MonoBehaviour
{
    [Header("ManaBlock Settings")]
    public string blockID = "ManaBlock_01";
    public SpriteRenderer indicatorSprite;
    public BoxCollider2D interactionTrigger;
    public float interactRange = 0.5f;
    public KeyCode interactKey = KeyCode.E;

    private bool hasTriggered = false;
    private bool playerInRange = false;
    private Transform player;

    private void Start()
    {
        if (indicatorSprite != null)
            indicatorSprite.enabled = false;

        // Pastikan trigger collider aktif
        if (interactionTrigger != null)
            interactionTrigger.isTrigger = true;

        // 🔹 Ambil status dari SaveSystem
        if (SaveSystem.Instance != null)
        {
            hasTriggered = SaveSystem.Instance.IsManaBlockTriggered(blockID);
            if (hasTriggered)
                Debug.Log($"🟡 ManaBlock '{blockID}' sudah pernah diaktifkan sebelumnya.");
        }
    }

    private void Update()
    {
        // Deteksi tombol E saat player berada di dekat block
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Debug.Log($"🎯 Tombol {interactKey} ditekan pada '{blockID}'!");
            StartCoroutine(RestoreFromManaBlock());
        }
    }

    // ============================================================
    // 🟡 Trigger detection untuk area interaksi
    // ============================================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            player = collision.transform;
            if (indicatorSprite != null)
                indicatorSprite.enabled = true;
            Debug.Log($"🟢 Player entered trigger of {blockID}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            if (indicatorSprite != null)
                indicatorSprite.enabled = false;
            Debug.Log($"🔴 Player exited trigger of {blockID}");
        }
    }

    // ============================================================
    // 🟢 Collision detection untuk SaveSpecial
    // ============================================================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // ❌ Jangan save kalau block ini sudah pernah aktif
            if (hasTriggered)
            {
                Debug.Log($"⚠️ ManaBlock '{blockID}' sudah diaktifkan sebelumnya, tidak menyimpan ulang.");
                return;
            }

            Debug.Log($"💾 Player collided with ManaBlock '{blockID}'");

            hasTriggered = true;
            SaveSystem.Instance?.SetManaBlockTriggered(blockID);

            if (ResourceManager.Instance != null && SaveSystem.Instance != null)
            {
                ResourceManager.Instance.FullRestoreMana();
                Transform p = collision.collider.transform;
                SaveSystem.Instance.SaveSpecial(blockID, p.position, ResourceManager.Instance.CurrentMana);
                SaveSystem.Instance.SetLastManaBlock(blockID);
                Debug.Log($"💾 ManaBlock '{blockID}' saved player & scene state!");
            }

            if (indicatorSprite != null)
                indicatorSprite.enabled = false;
        }
    }

    // ============================================================
    // 🔁 Restore dari save spesifik
    // ============================================================
    private IEnumerator RestoreFromManaBlock()
    {
        if (SaveSystem.Instance == null)
        {
            Debug.LogWarning("⚠️ SaveSystem not found!");
            yield break;
        }

        Debug.Log($"🔄 Restoring save from ManaBlock '{blockID}'...");
        yield return new WaitForSeconds(0.1f);

        SaveSystem.Instance.RestoreSpecial(blockID);
    }
}
