using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class DuplicatePlacementNew : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private DragSelection selectionManager;
    [SerializeField] private GridCursor gridCursor;
    [SerializeField] private Tilemap targetTilemap;

    [Header("VFX")]
    [SerializeField] private GameObject placeVfxPrefab;

    private List<GameObject> previewClones = new List<GameObject>();
    private bool isPlacing = false;
    private bool canPlace = true;

    // Event: bisa digunakan oleh sistem lain jika butuh tahu kapan objek baru ditempatkan
    public static event System.Action<List<GameObject>> OnObjectsPlaced;

    private List<GameObject> lastPlacedObjects = new List<GameObject>();
    public List<GameObject> GetLastPlacedObjects() => lastPlacedObjects;

    void Update()
    {
        if (!isPlacing)
        {
            // Mulai mode duplikasi
            if (Input.GetKeyDown(KeyBindings.DuplicateKey) && selectionManager.SelectedObjects.Count > 0)
            {
                StartPlacementMode();
            }
        }
        else
        {
            UpdatePreviewPosition();

            // Konfirmasi penempatan
            if (Input.GetKeyDown(KeyBindings.ConfirmKey))
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
                    Debug.Log("❌ Tidak bisa place: Objek bertabrakan!");
                }
            }

            // Cancel dengan klik kanan atau Esc
            else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }
    }

    // 🔹 Mulai mode duplikasi (membuat preview transparan)
    void StartPlacementMode()
    {
        isPlacing = true;
        selectionManager.IsSelectionEnabled = false;
        previewClones.Clear();

        foreach (var obj in selectionManager.SelectedObjects)
        {
            if (obj == null) continue;

            GameObject clone = Instantiate(obj, obj.transform.position, Quaternion.identity);

            // Nonaktifkan physics
            var col = clone.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            var rb = clone.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Static;

            // Jadikan transparan
            foreach (var sr in clone.GetComponentsInChildren<SpriteRenderer>())
                sr.color = new Color(1f, 1f, 1f, 0.5f);

            previewClones.Add(clone);
        }
    }

    // 🔹 Tempatkan duplikasi secara permanen
    void PlaceDuplicates()
    {
        List<GameObject> placedObjects = new List<GameObject>();

        foreach (var obj in previewClones)
        {
            if (obj == null) continue;

            // Reset warna
            foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
                sr.color = Color.white;

            obj.tag = "Selectable";
            placedObjects.Add(obj);

            if (placeVfxPrefab != null)
                Instantiate(placeVfxPrefab, obj.transform.position, Quaternion.identity);
        }

        OnObjectsPlaced?.Invoke(placedObjects);
        Debug.Log($"✅ Placed {placedObjects.Count} duplicated objects.");

        lastPlacedObjects = placedObjects;
        previewClones.Clear();
        isPlacing = false;
        selectionManager.IsSelectionEnabled = true;

        // 🔥 Kembali ke movement mode
        if (GameModeManager.Instance != null)
            GameModeManager.Instance.SwitchMode(GameMode.Movement);
    }

    // 🔹 Batalkan placement
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

    // 🔹 Hitung pivot untuk snap ke grid
    Vector3 CalcSnapPivot(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);

            Vector3 center = b.center;
            Vector3Int cell = targetTilemap.WorldToCell(center);
            return targetTilemap.GetCellCenterWorld(cell);
        }

        return obj.transform.position;
    }

    // 🔹 Update posisi preview duplikasi
    void UpdatePreviewPosition()
    {
        if (selectionManager.SelectedObjects.Count == 0) return;

        Vector3 mouseCellCenter = gridCursor.CurrentCellCenter;
        mouseCellCenter.z = 0f;

        Vector3 referencePos = CalcSnapPivot(selectionManager.SelectedObjects[0]);
        Vector3 offset = mouseCellCenter - referencePos;

        canPlace = true;
        int count = Mathf.Min(previewClones.Count, selectionManager.SelectedObjects.Count);

        for (int i = 0; i < count; i++)
        {
            if (previewClones[i] == null || selectionManager.SelectedObjects[i] == null) continue;

            Vector3 objRefPos = CalcSnapPivot(selectionManager.SelectedObjects[i]);
            Vector3 newPos = objRefPos + offset;
            previewClones[i].transform.position = newPos;

            // Cek overlap collider
            Collider2D col = previewClones[i].GetComponent<Collider2D>();
            if (col != null)
            {
                Bounds previewBounds = col.bounds;
                Collider2D[] hits = Physics2D.OverlapBoxAll(previewBounds.center, previewBounds.size * 0.95f, 0f);

                foreach (var hit in hits)
                {
                    if (hit == null) continue;
                    GameObject hitObj = hit.gameObject;

                    if (hitObj == previewClones[i]) continue;
                    if (previewClones.Contains(hitObj)) continue;
                    if (hitObj.CompareTag("ManaOrb")) continue;

                    if (previewBounds.Intersects(hit.bounds))
                    {
                        canPlace = false;
                        break;
                    }
                }
            }
        }

        // Update warna preview (hijau = bisa, merah = tidak)
        foreach (var obj in previewClones)
        {
            if (obj == null) continue;

            foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
                sr.color = canPlace ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
        }
    }
}