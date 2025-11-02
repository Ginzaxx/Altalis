using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class CutPlacement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private GridSelection gridSelection;
    [SerializeField] private GridCursor gridCursor;
    [SerializeField] private Tilemap targetTilemap;

    [Header("VFX")]
    [SerializeField] private GameObject placeVfxPrefab;

    [Header("Mouse & Gamepad Cursor")]
    [SerializeField] private float cursorMoveSpeed = 5f;
    private Vector3 cursorPosition;
    private Vector2 cursorMoveInput;

    private List<GameObject> previewClones = new List<GameObject>();
    private List<GameObject> originals = new List<GameObject>();
    private bool isPlacing = false;
    private bool canPlace = true;

    // Event: Tell Other Systems when a New Object was placed
    public static event System.Action<List<GameObject>> OnObjectsCutPlaced;
    public List<GameObject> GetLastPlacedObjects() => lastPlacedObjects;
    private List<GameObject> lastPlacedObjects = new List<GameObject>();

    void Update()
    {
        if (isPlacing) UpdatePreviewPosition();
    }

    // Start Cut Process
    public void StartPlacement(InputAction.CallbackContext CutKey)
    {
        if (CutKey.performed)
        {
            Debug.Log("Pressing Cut");
            if (!isPlacing && gridSelection.SelectedObjects.Count > 0)
            {
                Debug.Log("Starting Cut");
                isPlacing = true;
                gridSelection.IsSelectionEnabled = false;

                previewClones.Clear();
                originals.Clear();

                foreach (var obj in gridSelection.SelectedObjects)
                {
                    if (obj == null) continue;

                    originals.Add(obj);

                    GameObject clone = Instantiate(obj, obj.transform.position, Quaternion.identity);

                    var col = clone.GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;

                    var rb = clone.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.bodyType = RigidbodyType2D.Static;

                    foreach (var sr in clone.GetComponentsInChildren<SpriteRenderer>())
                        sr.color = new Color(1f, 1f, 1f, 0.5f);

                    previewClones.Add(clone);
                }
            }
        }
    }

    // Paste Cut Objects
    public void PlaceCutObjects(InputAction.CallbackContext PasteKey)
    {
        if (PasteKey.performed)
        {
            Debug.Log("Pressing Paste");
            if (isPlacing)
            {
                if (canPlace)
                {
                    if (ResourceManager.Instance != null && ResourceManager.Instance.TrySpendMana())
                    {
                        List<GameObject> placedObjects = new List<GameObject>();

                        // Paste New Objects
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
                                    Instantiate(placeVfxPrefab, obj.transform.position, Quaternion.identity);
                            }
                        }

                        // Delete Originals
                        foreach (var orig in originals)
                        {
                            var shrink = orig.GetComponent<ShrinkOnDestroy>();
                            if (shrink != null)
                                shrink.StartShrink();
                            else
                                Destroy(orig);
                        }

                        // Feedback
                        OnObjectsCutPlaced?.Invoke(placedObjects);
                        Debug.Log($"Cut-Placed {placedObjects.Count} objects. Originals deleted.");

                        lastPlacedObjects = placedObjects;

                        previewClones.Clear();
                        originals.Clear();
                        isPlacing = false;
                        gridSelection.IsSelectionEnabled = true;

                        // Return to Movement Mode
                        if (GameModeManager.Instance != null)
                            GameModeManager.Instance.SwitchMode(GameMode.Movement);
                    }
                    else
                    {
                        Debug.Log("Unable to Cut-Place: Not enough Mana!");
                        CancelPlacement();
                    }
                }
                else Debug.Log("Unable to Cut-Place: Object obstructed!");
            }
        }
    }

    public void CancelCutObjects(InputAction.CallbackContext CancelKey)
    {
        if (CancelKey.performed)
        {
            // Debug.Log("Pressing Cancel");
            CancelPlacement();
        }
    }

    public void OnMoveCursor(InputAction.CallbackContext Cursor)
    {
        cursorMoveInput = Cursor.ReadValue<Vector2>();
    }

    // Cancel Cut Process
    void CancelPlacement()
    {
        foreach (var obj in previewClones)
            if (obj != null) Destroy(obj);

        previewClones.Clear();
        originals.Clear();
        isPlacing = false;
        gridSelection.IsSelectionEnabled = true;
    }

    // Calculate Snap to Grid Pivot
    private Vector3 CalcSnapPivot(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        if (renderers == null || renderers.Length == 0)
            return obj.transform.position;

        Bounds b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        Vector3 center = b.center;

        Vector3Int cell = targetTilemap.WorldToCell(center);
        return targetTilemap.GetCellCenterWorld(cell);
    }

    // Update Preview Position
    void UpdatePreviewPosition()
    {
        if (originals.Count == 0) return;

        // 1. Mouse-Driven Cursor
        if (Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero)
            cursorPosition = gridCursor.CurrentCellCenter;
        // 2. Gamepad-Driven Cursor
        else if (cursorMoveInput.sqrMagnitude > 0.01f)
            cursorPosition += new Vector3(cursorMoveInput.x, cursorMoveInput.y, 0f) * cursorMoveSpeed * Time.deltaTime;

        // 3. Snap Cursor to Grid Cell
        Vector3Int cell = targetTilemap.WorldToCell(cursorPosition);
        Vector3 snappedPos = targetTilemap.GetCellCenterWorld(cell);
        cursorPosition = snappedPos;

        // Calculate Relative Offset with First Object
        Vector3 referencePos = CalcSnapPivot(originals[0]);
        Vector3 offset = snappedPos - referencePos;

        canPlace = true;

        int count = Mathf.Min(previewClones.Count, originals.Count);
        for (int i = 0; i < count; i++)
        {
            if (previewClones[i] == null || originals[i] == null) continue;

            Vector3 objRefPos = CalcSnapPivot(originals[i]);
            Vector3 newPos = objRefPos + offset;
            previewClones[i].transform.position = newPos;

            // Check Overlaps
            Collider2D col = previewClones[i].GetComponent<Collider2D>();
            if (col != null)
            {
                Bounds previewBounds = col.bounds;
                Collider2D[] hits = Physics2D.OverlapBoxAll(
                    previewBounds.center,
                    previewBounds.size * 0.95f,
                    0f);

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

        // Preview Colors
        foreach (var obj in previewClones)
        {
            if (obj != null)
            {
                foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.color = canPlace
                        ? new Color(0f, 1f, 0f, 0.5f) // Green
                        : new Color(1f, 0f, 0f, 0.5f); // Red
                }
            }
        }

        // (Optional) Update Position Visuals for Gamepad Users
        if (gridCursor != null)
            gridCursor.transform.position = cursorPosition;
    }
}
