using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class DuplicatePlacement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private DragSelection selectionManager;
    [SerializeField] private GridCursor gridCursor;
    [SerializeField] private Tilemap targetTilemap;

    [Header("VFX")]
    [SerializeField] private GameObject placeVfxPrefab;

    [Header("Mouse & Gamepad Cursor")]
    [SerializeField] private float cursorMoveSpeed = 5f;
    private Vector3 cursorPosition;
    private Vector2 cursorMoveInput;

    private List<GameObject> previewClones = new List<GameObject>();
    private bool isPlacing = false;
    private bool canPlace = true;

    // Event: Tell Other Systems when a New Object was placed
    public static event System.Action<List<GameObject>> OnObjectsPlaced;
    private List<GameObject> lastPlacedObjects = new List<GameObject>();
    public List<GameObject> GetLastPlacedObjects() => lastPlacedObjects;

    void Update()
    {
        if (isPlacing) UpdatePreviewPosition();
    }

    // Start Copy Process
    public void StartPlacementMode(InputAction.CallbackContext context)
    {
        if (context.performed && !isPlacing && selectionManager.SelectedObjects.Count > 0)
        {
            isPlacing = true;
            selectionManager.IsSelectionEnabled = false;

            previewClones.Clear();

            foreach (var obj in selectionManager.SelectedObjects)
            {
                if (obj == null) continue;

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

    // Paste Copy Objects
    public void PlaceCopyObjects(InputAction.CallbackContext context)
    {
        if (context.performed && isPlacing)
        {
            if (canPlace)
            {
                if (ResourceManager.Instance != null && ResourceManager.Instance.TrySpendMana())
                {
                    List<GameObject> placedObjects = new List<GameObject>();

                    foreach (var obj in previewClones)
                    {
                        if (obj == null) continue;

                        foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
                            sr.color = Color.white;

                        obj.tag = "Selectable";
                        placedObjects.Add(obj);

                        if (placeVfxPrefab != null)
                            Instantiate(placeVfxPrefab, obj.transform.position, Quaternion.identity);
                    }

                    OnObjectsPlaced?.Invoke(placedObjects);
                    Debug.Log($"✅ Placed {placedObjects.Count} Duplicated objects.");

                    lastPlacedObjects = placedObjects;
                    previewClones.Clear();
                    isPlacing = false;
                    selectionManager.IsSelectionEnabled = true;

                    // Return to Movement Mode
                    if (GameModeManager.Instance != null)
                        GameModeManager.Instance.SwitchMode(GameMode.Movement);
                }
                else
                {
                    Debug.Log("❌ Unable to Place: Not enough Mana!");
                    CancelPlacement();
                }
            }
            else Debug.Log("❌ Unable to Place: Object Obstructed!");
        }
    }

    public void CancelCutObjects(InputAction.CallbackContext context)
    {
        if (context.performed) CancelPlacement();
    }

    // Cancel Placement
    void CancelPlacement()
    {
        foreach (var obj in previewClones)
            if (obj != null) Destroy(obj);

        previewClones.Clear();
        isPlacing = false;
        selectionManager.IsSelectionEnabled = true;
    }

    public void OnMoveCursor(InputAction.CallbackContext ctx)
    {
        cursorMoveInput = ctx.ReadValue<Vector2>();
    }

    // Calculate Snap to Grip Pivot
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
        if (selectionManager.SelectedObjects.Count == 0) return;

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
        Vector3 referencePos = CalcSnapPivot(selectionManager.SelectedObjects[0]);
        Vector3 offset = snappedPos - referencePos;

        canPlace = true;

        int count = Mathf.Min(previewClones.Count, selectionManager.SelectedObjects.Count);
        for (int i = 0; i < count; i++)
        {
            if (previewClones[i] == null || selectionManager.SelectedObjects[i] == null) continue;

            Vector3 objRefPos = CalcSnapPivot(selectionManager.SelectedObjects[i]);
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