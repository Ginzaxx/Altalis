using UnityEngine;
using System.Collections.Generic;

public class DragSelection : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] public RectTransform selectionBoxUI; // Made public for GameModeManager access
    public bool IsSelectionEnabled { get; set; } = true;

    private Vector2 startPos;
    private Vector2 endPos;
    private bool isDragging = false;

    [Header("Slow Motion Settings")]
    public bool useUnscaledTimeForUI = true; // Makes UI responsive during slow motion

    public List<GameObject> SelectedObjects { get; private set; } = new List<GameObject>();
    private List<GameObject> previewSelection = new List<GameObject>();

    void Update()
    {
        // Only work if selection is enabled and we're in selection mode
        if (!IsSelectionEnabled || !enabled) return;
        
        // Don't process if right mouse button is being used for mode switching
        if (Input.GetMouseButtonDown(1)) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            StartSelection();
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateSelection();
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndSelection();
        }
    }

    void StartSelection()
    {
        startPos = Input.mousePosition;
        isDragging = true;
        if (selectionBoxUI != null)
        {
            selectionBoxUI.gameObject.SetActive(true);
        }
    }

    void UpdateSelection()
    {
        endPos = Input.mousePosition;
        DrawSelectionBox();
        
        // Update selection preview at a reasonable rate even during slow motion
        if (useUnscaledTimeForUI)
        {
            // Use unscaled time to ensure UI updates smoothly during slow motion
            PreviewSelection();
        }
        else
        {
            PreviewSelection();
        }
    }

    void EndSelection()
    {
        if (isDragging)
        {
            ConfirmSelection();
            isDragging = false;
            if (selectionBoxUI != null)
            {
                selectionBoxUI.gameObject.SetActive(false);
            }
        }
    }

    void DrawSelectionBox()
    {
        if (selectionBoxUI == null) return;

        Canvas parentCanvas = selectionBoxUI.GetComponentInParent<Canvas>();
        RectTransform canvasRect = parentCanvas.transform as RectTransform;

        Vector2 localStartPos;
        Vector2 localEndPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, startPos,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            out localStartPos
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, endPos,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            out localEndPos
        );

        Vector2 boxCenter = (localStartPos + localEndPos) / 2f;
        Vector2 boxSize = new Vector2(Mathf.Abs(localEndPos.x - localStartPos.x), Mathf.Abs(localEndPos.y - localStartPos.y));

        selectionBoxUI.anchoredPosition = boxCenter;
        selectionBoxUI.sizeDelta = boxSize;
    }

    void PreviewSelection()
    {
        // Clear previous preview highlighting
        foreach (var obj in previewSelection)
        {
            if (obj != null && !SelectedObjects.Contains(obj))
            {
                var sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.white;
            }
        }
        previewSelection.Clear();

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

            // 🔥 Kalau collider anak tapi parent selectable → naik ke parent
            if (target.transform.parent != null && target.transform.parent.CompareTag("Selectable"))
            {
                target = target.transform.parent.gameObject;
            }

            if (target.CompareTag("Selectable") && !previewSelection.Contains(target))
            {
                previewSelection.Add(target);

                // 🔥 Warnai semua child renderer jadi cyan
                foreach (var sr in target.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.color = Color.cyan;
                }
            }
        }
    }

    void ConfirmSelection()
    {
        foreach (var obj in SelectedObjects)
            {
                if (obj != null)
                {
                    // 🔥 Reset warna semua child juga
                    foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
                    {
                        sr.color = Color.white;
                    }
                }
            }

        SelectedObjects.Clear();

        // Apply new selection
        foreach (var obj in previewSelection)
        {
            if (obj != null)
            {
                SelectedObjects.Add(obj);

                // 🔥 Warnai semua child saat parent terseleksi
                foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.color = Color.yellow;
                }
            }
        }

        previewSelection.Clear();

        Debug.Log("Selected " + SelectedObjects.Count + " objects");
    }

    // Method to clear all selections (useful when switching modes)
    public void ClearSelection()
    {
        foreach (var obj in SelectedObjects)
        {
            if (obj != null)
            {
                // 🔥 Reset semua anak juga
                foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.color = Color.white;
                }
            }
        }
        SelectedObjects.Clear();
        
        foreach (var obj in previewSelection)
        {
            if (obj != null)
            {
                foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.color = Color.white;
                }
            }
        }
        previewSelection.Clear();
        
        if (selectionBoxUI != null)
        {
            selectionBoxUI.gameObject.SetActive(false);
        }
    }

    // Called when the script is disabled
    void OnDisable()
    {
        ClearSelection();
        isDragging = false;
    }
}