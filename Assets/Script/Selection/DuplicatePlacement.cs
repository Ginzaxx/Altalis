using UnityEngine;
using System.Collections.Generic;

public class DuplicatePlacement : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private DragSelection selectionManager;

    private List<GameObject> previewClones = new List<GameObject>();
    private bool isPlacing = false;

    void Update()
    {
        if (!isPlacing)
        {
            if (Input.GetKeyDown(KeyCode.V) && selectionManager.SelectedObjects.Count > 0)
            {
                StartPlacementMode();
            }
        }
        else
        {
            UpdatePreviewPosition();

            if (Input.GetMouseButtonDown(0)) // Confirm
            {
                PlaceDuplicates();
            }
            else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) // Cancel
            {
                CancelPlacement();
            }
        }
    }

    void StartPlacementMode()
    {
        isPlacing = true;
        previewClones.Clear();

        foreach (var obj in selectionManager.SelectedObjects)
        {
            if (obj != null)
            {
                GameObject clone = Instantiate(obj, obj.transform.position, Quaternion.identity);
                var sr = clone.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.5f); // ghost
                previewClones.Add(clone);
            }
        }
    }

    void UpdatePreviewPosition()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector3 offset = mouseWorld - selectionManager.SelectedObjects[0].transform.position;

        for (int i = 0; i < previewClones.Count; i++)
        {
            if (previewClones[i] != null && selectionManager.SelectedObjects[i] != null)
            {
                previewClones[i].transform.position = selectionManager.SelectedObjects[i].transform.position + offset;
            }
        }
    }

    void PlaceDuplicates()
    {
        foreach (var obj in previewClones)
        {
            if (obj != null)
            {
                var sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.white;
                obj.tag = "Selectable"; 
            }
        }
        previewClones.Clear();
        isPlacing = false;
    }

    void CancelPlacement()
    {
        foreach (var obj in previewClones)
        {
            if (obj != null) Destroy(obj);
        }
        previewClones.Clear();
        isPlacing = false;
    }
}
