using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridSelection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Camera cam;
    [SerializeField] public Grid grid;
    [SerializeField] public RectTransform selectionBoxUI;
    public bool IsSelectionEnabled { get; set; } = true;

    [Header("Slow Motion Settings")]
    public bool useUnscaledTimeForUI = true;

    private bool IsDragging = false;
    private float distancePos = 20f;
    private Vector2 startPos;
    private Vector2 endPos;

    public List<GameObject> SelectedObjects { get; private set; } = new List<GameObject>();
    private List<GameObject> SelectionPreview = new List<GameObject>();

    // Limit dari ResourceManager
    private int MaxSelectable => ResourceManager.Instance != null ? ResourceManager.Instance.SelectLimit : 3;

    void Update()
    {
        if (!IsSelectionEnabled || !enabled) return;
        if (Input.GetMouseButtonDown(1)) return; // Ignore Right Click

        // Tap (Click without Drag)
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            IsDragging = true;

            if (selectionBoxUI != null)
                selectionBoxUI.gameObject.SetActive(false);
        }

        if (Input.GetMouseButton(0) && IsDragging)
        {
            endPos = Input.mousePosition;

            // If Big Drag → Drag Select
            if (Vector2.Distance(startPos, endPos) > distancePos)
            {
                if (selectionBoxUI != null)
                    selectionBoxUI.gameObject.SetActive(true);

                UpdateSelectionBox();
                PreviewSelection();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            IsDragging = false;

            if (selectionBoxUI != null)
                selectionBoxUI.gameObject.SetActive(false);

            // If Small Drag
            if (Vector2.Distance(startPos, Input.mousePosition) < distancePos)
                HandleTapSelection();
            else
                ConfirmSelection();
        }
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!IsSelectionEnabled || !enabled) return;

                GridCursor cursor = FindObjectOfType<GridCursor>();
                if (cursor == null || grid == null) return;

                Vector3 worldPos = cursor.CurrentCellCenter;
                Collider2D hit = Physics2D.OverlapPoint(worldPos);

            if (hit != null)
            {
                GameObject target = hit.gameObject;

                if (target.transform.parent != null && target.transform.parent.CompareTag("Selectable"))
                    target = target.transform.parent.gameObject;

                if (target.CompareTag("Selectable"))
                {
                    if (SelectedObjects.Contains(target)) DeselectObject(target);
                    else if (SelectedObjects.Count < MaxSelectable) SelectObject(target);
                    else Debug.Log($"Sudah mencapai batas seleksi: {MaxSelectable}");

                    return;
                }
            }
            else ClearSelection();
        }
    }

    // Tap Selection (Click 1 Object without Reset)
    void HandleTapSelection()
    {
        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

        if (hit != null)
        {
            GameObject target = hit.gameObject;

            if (target.transform.parent != null && target.transform.parent.CompareTag("Selectable"))
                target = target.transform.parent.gameObject;

            if (target.CompareTag("Selectable"))
            {
                // Toggle on/off selection
                if (SelectedObjects.Contains(target)) DeselectObject(target);
                else if (SelectedObjects.Count < MaxSelectable) SelectObject(target);
                else Debug.Log($"Sudah mencapai batas seleksi: {MaxSelectable}");

                return; // Jangan reset selection
            }
        }
        // If Empty → Reset All
        ClearSelection();
    }

    void UpdateSelectionBox()
    {
        if (selectionBoxUI == null) return;

        Canvas parentCanvas = selectionBoxUI.GetComponentInParent<Canvas>();
        RectTransform canvasRect = parentCanvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle
        (
            canvasRect, startPos,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            out Vector2 localStartPos
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle
        (
            canvasRect, endPos,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            out Vector2 localEndPos
        );

        Vector2 boxCenter = (localStartPos + localEndPos) / 2f;
        Vector2 boxSize = new Vector2(Mathf.Abs(localEndPos.x - localStartPos.x), Mathf.Abs(localEndPos.y - localStartPos.y));

        selectionBoxUI.anchoredPosition = boxCenter;
        selectionBoxUI.sizeDelta = boxSize;
    }

    void PreviewSelection()
    {
        // Reset Previous Preview Color
        foreach (var obj in SelectionPreview)
            if (obj != null && !SelectedObjects.Contains(obj))
                SetColor(obj, Color.white);
        SelectionPreview.Clear();

        Vector3 p1 = cam.ScreenToWorldPoint(startPos);
        Vector3 p2 = cam.ScreenToWorldPoint(endPos);

        float minX = Mathf.Min(p1.x, p2.x);
        float maxX = Mathf.Max(p1.x, p2.x);
        float minY = Mathf.Min(p1.y, p2.y);
        float maxY = Mathf.Max(p1.y, p2.y);

        Collider2D[] hits = Physics2D.OverlapAreaAll(new Vector2(minX, minY), new Vector2(maxX, maxY));

        foreach (Collider2D hit in hits)
        {
            GameObject target = hit.gameObject;
            if (target.transform.parent != null && target.transform.parent.CompareTag("Selectable"))
                target = target.transform.parent.gameObject;

            if (target.CompareTag("Selectable") && !SelectionPreview.Contains(target))
            {
                SelectionPreview.Add(target);
                SetColor(target, Color.cyan);
            }
        }
    }

    // Paste Selection List
    void ConfirmSelection()
    {
        foreach (var obj in SelectionPreview)
        {
            if (obj == null) continue;

            if (!SelectedObjects.Contains(obj))
                if (SelectedObjects.Count < MaxSelectable) 
                    SelectObject(obj);
        }

        // Reset Preview Color
        foreach (var obj in SelectionPreview)
            if (obj != null && !SelectedObjects.Contains(obj))
                SetColor(obj, Color.white);
        SelectionPreview.Clear();

        Debug.Log($"Selected {SelectedObjects.Count} objects");
    }

    // Add to Selection List
    void SelectObject(GameObject obj)
    {
        if (!SelectedObjects.Contains(obj))
        {
            SelectedObjects.Add(obj);
            SetColor(obj, Color.yellow);
        }
    }

    // Remove from Selection List
    void DeselectObject(GameObject obj)
    {
        if (SelectedObjects.Contains(obj))
        {
            SetColor(obj, Color.white);
            SelectedObjects.Remove(obj);
        }
    }

    // Set Color for all Child Renders
    void SetColor(GameObject obj, Color color)
    {
        foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
            sr.color = color;
    }

    // Clear All Selection
    public void ClearSelection()
    {
        foreach (var obj in SelectedObjects)
            if (obj != null)
                SetColor(obj, Color.white);
        SelectedObjects.Clear();

        foreach (var obj in SelectionPreview)
            if (obj != null)
                SetColor(obj, Color.white);
        SelectionPreview.Clear();

        if (selectionBoxUI != null)
            selectionBoxUI.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        ClearSelection();
        IsDragging = false;
    }
}