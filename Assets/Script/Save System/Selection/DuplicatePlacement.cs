using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;

public class DuplicatePlacement : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private DragSelection selectionManager;
    [SerializeField] private GridCursor gridCursor;
    [SerializeField] private Tilemap targetTilemap;

    private List<GameObject> previewClones = new List<GameObject>();
    private bool isPlacing = false;
    private bool canPlace = true;

    // Aflah broadcast signal for placement
    public static event System.Action<List<GameObject>> OnObjectsPlaced;

    private List<GameObject> lastPlacedObjects = new List<GameObject>();
    public List<GameObject> GetLastPlacedObjects() => lastPlacedObjects;

    void Update()
    {
        if (!isPlacing)
        {
            if (Input.GetKeyDown(KeyBindings.DuplicateKey) && selectionManager.SelectedObjects.Count > 0)
            {
                StartPlacementMode();
            }
        }
        else
        {
            UpdatePreviewPosition();

            if (Input.GetKeyDown(KeyBindings.ConfirmKey)) // Confirm
            {
                if (canPlace)
                {
                    if (ResourceManager.Instance != null && ResourceManager.Instance.TrySpendMana())
                    {
                        PlaceDuplicates();
                    }
                    else
                    {
                        Debug.Log("❌ Tidak bisa place: Mana habis!");
                        CancelPlacement();
                    }
                }
                else
                {
                    Debug.Log("❌ Tidak bisa place: objek bertabrakan!");
                }
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

                // Matikan collider & rigidbody supaya tidak ikut physics
                var col = clone.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                var rb = clone.GetComponent<Rigidbody2D>();
                if (rb != null) rb.bodyType = RigidbodyType2D.Static;

                // Bikin transparan
                foreach (var sr in clone.GetComponentsInChildren<SpriteRenderer>())
                    sr.color = new Color(1f, 1f, 1f, 0.5f);

                previewClones.Add(clone);
            }
        }
    }

    // 🔥 Helper function buat hitung pivot snap ke grid
    private Vector3 CalcSnapPivot(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            Vector3 center = b.center;

            // Snap ke cell center tilemap
            Vector3Int cell = targetTilemap.WorldToCell(center);
            return targetTilemap.GetCellCenterWorld(cell);
        }
        return obj.transform.position;
    }

    void UpdatePreviewPosition()
    {
        if (selectionManager.SelectedObjects.Count == 0) return;

        // 🔥 Ambil posisi grid cursor (target snap posisi)
        Vector3 mouseCellCenter = gridCursor.CurrentCellCenter;
        mouseCellCenter.z = 0f;

        // 🔥 Pivot reference dari object pertama
        Vector3 referencePos = CalcSnapPivot(selectionManager.SelectedObjects[0]);
        Vector3 offset = mouseCellCenter - referencePos;

        canPlace = true;

        // Geser semua preview clone sesuai offset
        int count = Mathf.Min(previewClones.Count, selectionManager.SelectedObjects.Count);
        for (int i = 0; i < count; i++)
        {
            if (previewClones[i] == null || selectionManager.SelectedObjects[i] == null) continue;

            Vector3 objRefPos = CalcSnapPivot(selectionManager.SelectedObjects[i]);
            Vector3 newPos = objRefPos + offset;
            previewClones[i].transform.position = newPos;

            // ✅ Cek overlap
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

                    if (previewBounds.Intersects(hit.bounds))
                    {
                        if (hitObj.CompareTag("ManaOrb")) continue;

                        canPlace = false;
                        break;
                    }
                }
            }
        }

        // 🔥 Update warna preview
        foreach (var obj in previewClones)
        {
            if (obj != null)
            {
                foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.color = canPlace ? new Color(0f, 1f, 0f, 0.5f) // hijau
                                        : new Color(1f, 0f, 0f, 0.5f); // merah
                }
            }
        }
    }

    void PlaceDuplicates()
    {
        List<GameObject> placedObjects = new List<GameObject>();

        foreach (var obj in previewClones)
        {
            if (obj != null)
            {
                foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
                    sr.color = Color.white;

                obj.tag = "Selectable";
                placedObjects.Add(obj);
            }
        }

        OnObjectsPlaced?.Invoke(placedObjects);
        Debug.Log($"✅ Event triggered for {placedObjects.Count} placed objects.");

        lastPlacedObjects = placedObjects;

        previewClones.Clear();
        isPlacing = false;
        selectionManager.IsSelectionEnabled = true;

        // 🔥 Langsung balik ke Movement Mode
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SwitchMode(GameMode.Movement);
        }
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
