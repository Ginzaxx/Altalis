using UnityEngine;

public class GridSelection : MonoBehaviour
{
    [SerializeField] private GridCursor cursor;
    [SerializeField] private Camera cam;

    private GameObject selectedObject;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectAtCursor();
        }
    }

    void SelectAtCursor()
    {
        // Reset highlight kalau ada object lama
        if (selectedObject != null)
        {
            SpriteRenderer oldSr = selectedObject.GetComponent<SpriteRenderer>();
            if (oldSr != null) oldSr.color = Color.white;
        }

        Vector3 worldPos = cursor.CurrentCellCenter;
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Selectable"))
        {
            selectedObject = hit.collider.gameObject;
            Debug.Log("Selected: " + selectedObject.name);

            SpriteRenderer sr = selectedObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.yellow; // highlight
        }
        else
        {
            selectedObject = null;
        }
    }
}
