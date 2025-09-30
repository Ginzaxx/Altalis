using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class CutPlacement : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private DragSelection selectionManager;
    [SerializeField] private GridCursor gridCursor;
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private GameObject placeVfxPrefab;

    private List<GameObject> previewClones = new List<GameObject>();
    private List<GameObject> originals = new List<GameObject>();
    private bool isPlacing = false;
    private bool canPlace = true;

    public static event System.Action<List<GameObject>> OnObjectsCutPlaced;

    private List<GameObject> lastPlacedObjects = new List<GameObject>();
    public List<GameObject> GetLastPlacedObjects() => lastPlacedObjects;

    void Update()
    {
        if (!isPlacing)
        {
            if (Input.GetKeyDown(KeyBindings.CutKey) && selectionManager.SelectedObjects.Count > 0)
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
                        PlaceCutObjects();
                    }
                    else
                    {
                        Debug.Log("❌ Tidak bisa cut-place: Mana habis!");
                        CancelPlacement();
                    }
                }
                else
                {
                    Debug.Log("❌ Tidak bisa cut-place: objek bertabrakan!");
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
        originals.Clear();

        foreach (var obj in selectionManager.SelectedObjects)
        {
            if (obj != null)
            {
                originals.Add(obj);

                GameObject clone = Instantiate(obj, obj.transform.position, Quaternion.identity);

                // Matikan collider & rigidbody
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

    private Vector3 CalcSnapPivot(GameObject obj)
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

    void UpdatePreviewPosition()
    {
        if (originals.Count == 0) return;

        Vector3 mouseCellCenter = gridCursor.CurrentCellCenter;
        mouseCellCenter.z = 0f;

        Vector3 referencePos = CalcSnapPivot(originals[0]);
        Vector3 offset = mouseCellCenter - referencePos;

        canPlace = true;

        int count = Mathf.Min(previewClones.Count, originals.Count);
        for (int i = 0; i < count; i++)
        {
            if (previewClones[i] == null || originals[i] == null) continue;

            Vector3 objRefPos = CalcSnapPivot(originals[i]);
            Vector3 newPos = objRefPos + offset;
            previewClones[i].transform.position = newPos;

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

                    if (previewBounds.Intersects(hit.bounds))
                    {
                        if (hitObj.CompareTag("ManaOrb")) continue;

                        canPlace = false;
                        break;
                    }
                }
            }
        }

        foreach (var obj in previewClones)
        {
            if (obj != null)
            {
                foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.color = canPlace ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
                }
            }
        }
    }

  void PlaceCutObjects()
    {
        List<GameObject> placedObjects = new List<GameObject>();

        foreach (var obj in previewClones)
        {
            if (obj != null)
            {
                foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
                    sr.color = Color.white;

                var col = obj.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;

                var rb = obj.GetComponent<Rigidbody2D>();
                if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

                obj.tag = "Selectable";
                placedObjects.Add(obj);

                if (placeVfxPrefab != null)
                {
                    Instantiate(placeVfxPrefab, obj.transform.position, Quaternion.identity);
                }
            }
        }

        foreach (var orig in originals)
        {
            if (orig != null)
            {
                var dissolve = orig.GetComponent<DissolveOnDestroy>();
                if (dissolve != null)
                {
                    dissolve.StartDissolve();
                }
                else
                {
                    Destroy(orig);
                }
            }
        }

        OnObjectsCutPlaced?.Invoke(placedObjects);
        Debug.Log($"✂️ Cut-placed {placedObjects.Count} objects. Originals deleted.");

        lastPlacedObjects = placedObjects;

        previewClones.Clear();
        originals.Clear();
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
        originals.Clear();
        isPlacing = false;
        selectionManager.IsSelectionEnabled = true;
    }
}
