using UnityEngine;
using System.Collections.Generic;

public class DuplicatePlacement : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private DragSelection selectionManager;
    [SerializeField] private GridCursor gridCursor; // referensi ke GridCursor

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
                if (canPlace)
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
        selectionManager.IsSelectionEnabled = false;

        previewClones.Clear();

        foreach (var obj in selectionManager.SelectedObjects)
        {
            if (obj != null)
            {
                GameObject clone = Instantiate(obj, obj.transform.position, Quaternion.identity);
                var sr = clone.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.5f);
                previewClones.Add(clone);
            }
        }
    }

    void UpdatePreviewPosition()
    {
        if (selectionManager.SelectedObjects.Count == 0) return;

        // 🔥 Ambil posisi grid cursor
        Vector3 mouseCellCenter = gridCursor.CurrentCellCenter;
        mouseCellCenter.z = 0f;

        // Hitung offset dari object pertama
        Vector3 offset = mouseCellCenter - selectionManager.SelectedObjects[0].transform.position;

        canPlace = true;

        int count = Mathf.Min(previewClones.Count, selectionManager.SelectedObjects.Count);
        for (int i = 0; i < count; i++)
        {
            if (previewClones[i] == null || selectionManager.SelectedObjects[i] == null) continue;

            // Geser ke posisi baru
            Vector3 newPos = selectionManager.SelectedObjects[i].transform.position + offset;
            previewClones[i].transform.position = newPos;

            // ✅ Cek overlap pakai bounds
            Collider2D col = previewClones[i].GetComponent<Collider2D>();
            if (col != null)
            {
                Bounds previewBounds = col.bounds;

                Collider2D[] hits = Physics2D.OverlapBoxAll(previewBounds.center, previewBounds.size * 0.95f, 0f);

                foreach (var hit in hits)
                {
                    if (hit == null) continue;
                    GameObject hitObj = hit.gameObject;

                    if (hitObj == previewClones[i]) continue;      // abaikan diri sendiri
                    if (previewClones.Contains(hitObj)) continue;  // abaikan sesama preview

                    // ❌ Jangan abaikan original selection (fix utama di sini!)
                    // artinya: kalau overlap dengan original object → tidak boleh place

                    if (previewBounds.Intersects(hit.bounds))
                    {
                        canPlace = false;
                        break;
                    }
                }
            }
        }

        // Ubah warna preview
        foreach (var obj in previewClones)
        {
            if (obj != null)
            {
                var sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (canPlace) sr.color = new Color(0f, 1f, 0f, 0.5f); // hijau
                    else sr.color = new Color(1f, 0f, 0f, 0.5f);          // merah
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
        selectionManager.IsSelectionEnabled = true;
    }

    void CancelPlacement()
    {
        foreach (var obj in previewClones)
        {
            if (obj != null) Destroy(obj);
        }
        previewClones.Clear();
        isPlacing = false;
        selectionManager.IsSelectionEnabled = true;
    }
}
