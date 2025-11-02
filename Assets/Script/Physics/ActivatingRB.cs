using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mengaktifkan Rigidbody2D & Collider2D pada object yang baru ditempatkan oleh CopyPlacement.
/// Aman digunakan bersama sistem seleksi & peluncuran (tidak merusak velocity/constraints).
/// </summary>
public class ActivatingRB : MonoBehaviour
{
    private void OnEnable()
    {
        // ✅ Subscribe to event dari CopyPlacement
        CopyPlacement.OnObjectsPlaced += HandleObjectsPlaced;
    }

    private void OnDisable()
    {
        // ❌ Unsubscribe saat disable agar tidak leak
        CopyPlacement.OnObjectsPlaced -= HandleObjectsPlaced;
    }

    private void HandleObjectsPlaced(List<GameObject> placedObjects)
    {
        if (placedObjects == null || placedObjects.Count == 0)
            return;

        Debug.Log($"[ActivatingRB] Received event for {placedObjects.Count} objects at frame {Time.frameCount}");

        foreach (GameObject obj in placedObjects)
        {
            if (obj == null) continue;

            // --- Collider2D ---
            Collider2D col = obj.GetComponent<Collider2D>();
            if (col != null)
            {
                if (!col.enabled)
                {
                    col.enabled = true;
                    Debug.Log($"[ActivatingRB] Enabled collider on {obj.name}");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ {obj.name} has no Collider2D!");
            }

            // --- Rigidbody2D ---
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Simpan velocity & constraint sebelum ubah tipe
                Vector2 oldVel = rb.velocity;
                RigidbodyConstraints2D oldConstraints = rb.constraints;

                // Hanya ubah bodyType jika belum Dynamic
                if (rb.bodyType != RigidbodyType2D.Dynamic)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    Debug.Log($"[ActivatingRB] Activated Rigidbody2D on {obj.name}");
                }

                // Pastikan tetap aktif
                rb.simulated = true;
                rb.isKinematic = false;
                rb.gravityScale = rb.gravityScale; // tetap pakai nilai lama

                // Pulihkan velocity & constraints agar tidak “membekukan” object
                rb.velocity = oldVel;
                rb.constraints = oldConstraints;
            }
            else
            {
                Debug.LogWarning($"⚠️ {obj.name} has no Rigidbody2D!");
            }
        }
    }
}
