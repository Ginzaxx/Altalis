using System.Collections.Generic;
using UnityEngine;

public class ActivatingRB : MonoBehaviour
{
    private void OnEnable()
    {
        // ✅ Subscribe to the event when this object is enabled/activated
        DuplicatePlacement.OnObjectsPlaced += HandleObjectsPlaced;
    }

    private void OnDisable()
    {
        // ❌ Always unsubscribe to prevent errors and memory leaks
        DuplicatePlacement.OnObjectsPlaced -= HandleObjectsPlaced;
    }

    /// <summary>
    /// This method is called by the OnObjectsPlaced event from DuplicatePlacement.
    /// </summary>
    /// <param name="placedObjects">The list of objects that were just created.</param>
    private void HandleObjectsPlaced(List<GameObject> placedObjects)
    {
        Debug.Log($"ActivatingRB received event for {placedObjects.Count} objects.");

        foreach (GameObject obj in placedObjects)
        {
            if (obj == null) continue;

            // 🔥 Cek dulu collider ada atau tidak
            BoxCollider2D col = obj.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.enabled = true;
            }
            else
            {
                Debug.LogWarning($"⚠️ Object {obj.name} has no BoxCollider2D.");
            }

            // Cek Rigidbody2D
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                Debug.Log($"✅ Activated Rigidbody2D on {obj.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Object {obj.name} was placed but has no Rigidbody2D.");
            }
        }
    }
}