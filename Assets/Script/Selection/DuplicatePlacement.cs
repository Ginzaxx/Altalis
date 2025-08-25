using UnityEngine;
using System.Collections.Generic;

public class DuplicatePlacement : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private DragSelection selectionManager;

    private List<GameObject> previewClones = new List<GameObject>();
    private bool isPlacing = false;
    private bool canPlace = true;

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
                if (canPlace) // hanya bisa place jika tidak bertabrakan
                    PlaceDuplicates();
                else
                    Debug.Log("❌ Tidak bisa place: objek bertabrakan!");
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
        selectionManager.IsSelectionEnabled = false; // disable selection

        previewClones.Clear();

        foreach (var obj in selectionManager.SelectedObjects)
        {
            if (obj != null)
            {
                GameObject clone = Instantiate(obj, obj.transform.position, Quaternion.identity);
                var sr = clone.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.5f); // ghost transparan
                previewClones.Add(clone);
            }
        }
    }

    void UpdatePreviewPosition()
    {
        if (selectionManager.SelectedObjects.Count == 0) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector3 offset = mouseWorld - selectionManager.SelectedObjects[0].transform.position;
        bool isOffset = offset.sqrMagnitude > 0.01f; // cek apakah sudah digeser

        canPlace = true;

        int count = Mathf.Min(previewClones.Count, selectionManager.SelectedObjects.Count);
        for (int i = 0; i < count; i++)
        {
            if (previewClones[i] == null || selectionManager.SelectedObjects[i] == null) continue;

            // geser clone ke posisi baru
            previewClones[i].transform.position = selectionManager.SelectedObjects[i].transform.position + offset;

            // cek tabrakan
            Collider2D col = previewClones[i].GetComponent<Collider2D>();
            if (col != null)
            {
                Collider2D[] results = new Collider2D[10];
                ContactFilter2D filter = new ContactFilter2D();
                filter.NoFilter();
                int hits = Physics2D.OverlapCollider(col, filter, results);

                for (int h = 0; h < hits; h++)
                {
                    if (results[h] == null) continue;
                    GameObject hitObj = results[h].gameObject;

                    if (hitObj == previewClones[i]) continue;        // abaikan dirinya sendiri
                    if (previewClones.Contains(hitObj)) continue;    // abaikan sesama preview

                    // jika sudah geser, jangan boleh overlap dengan original
                    if (isOffset && selectionManager.SelectedObjects.Contains(hitObj))
                    {
                        canPlace = false;
                        break;
                    }

                    // overlap dengan object lain juga tidak boleh
                    if (!selectionManager.SelectedObjects.Contains(hitObj))
                    {
                        canPlace = false;
                        break;
                    }
                }
            }
        }

        // update warna preview sesuai status
        foreach (var obj in previewClones)
        {
            if (obj != null)
            {
                var sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (canPlace) sr.color = new Color(0f, 1f, 0f, 0.5f); // hijau transparan
                    else sr.color = new Color(1f, 0f, 0f, 0.5f); // merah transparan
                }
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
        selectionManager.IsSelectionEnabled = true; // aktifkan selection lagi
    }

    void CancelPlacement()
    {
        foreach (var obj in previewClones)
        {
            if (obj != null) Destroy(obj);
        }
        previewClones.Clear();
        isPlacing = false;
        selectionManager.IsSelectionEnabled = true; // aktifkan selection lagi
    }
}
