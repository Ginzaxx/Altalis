using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ManaBlock : MonoBehaviour
{
    [Header("ManaBlock Settings")]
    public string blockID = "ManaBlock_01"; // unik per blok
    public SpriteRenderer indicatorSprite;  // sprite “Press E” atau efek visual
    public float interactRange = 0.5f;       // jarak player untuk interaksi
    public KeyCode interactKey = KeyCode.E;  // tombol interaksi

    private bool hasTriggered = false;
    private bool playerInRange = false;
    private Transform player;

private void Start()
{
    if (indicatorSprite != null)
        indicatorSprite.enabled = false;

    // 🔹 Cek status trigger dari SaveSystem
    if (SaveSystem.Instance != null && SaveSystem.Instance.IsManaBlockTriggered(blockID))
    {
        hasTriggered = true;
    }
}


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasTriggered) return;

        if (collision.collider.CompareTag("Player"))
        {
            hasTriggered = true;
            SaveSystem.Instance?.SetManaBlockTriggered(blockID);

        if (ResourceManager.Instance != null && SaveSystem.Instance != null)
        {
            Transform p = collision.collider.transform;
            SaveSystem.Instance.SaveSpecial(blockID, p.position, ResourceManager.Instance.CurrentMana);
            Debug.Log($"💾 ManaBlock '{blockID}' saved player & scene state!");
        }

            ResourceManager.Instance?.FullRestoreMana();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            player = collision.transform;
            if (indicatorSprite != null)
                indicatorSprite.enabled = true;
            SoundManager.PlaySound("Checkpoint", 0.8f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (indicatorSprite != null)
                indicatorSprite.enabled = false;
        }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.performed && playerInRange)
        {
            StartCoroutine(RestoreFromManaBlock());
        }
    }

    private IEnumerator RestoreFromManaBlock()
    {
        if (SaveSystem.Instance == null)
        {
            Debug.LogWarning("⚠️ SaveSystem not found!");
            yield break;
        }

        Debug.Log($"🔄 Restoring save from ManaBlock '{blockID}'...");
        yield return new WaitForSeconds(0.1f); // delay kecil biar aman

        SaveSystem.Instance.RestoreSpecial(blockID);
    }
}
