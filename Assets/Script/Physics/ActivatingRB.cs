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
            if (obj != null)
            {
                obj.GetComponent<BoxCollider2D>().enabled = true;
                // Find the Rigidbody2D component on the newly placed object
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    // This is where you implement your logic.
                    // For example, activate it by setting its body type.
                    // If it was Kinematic, setting it to Dynamic will make it respond to physics.
                    rb.bodyType = RigidbodyType2D.Dynamic;

                    // Or if it was sleeping, you can wake it up
                    // rb.WakeUp();

                    Debug.Log($"Activated Rigidbody2D on {obj.name}");
                }
                else
                {
                    Debug.LogWarning($"Object {obj.name} was placed but has no Rigidbody2D to activate.");
                }
            }
        }
    }
}