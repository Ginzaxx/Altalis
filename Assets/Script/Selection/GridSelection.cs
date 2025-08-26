using UnityEngine;
using System.Collections.Generic;

public class DragSelection : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private RectTransform selectionBoxUI;
    public bool IsSelectionEnabled { get; set; } = true;

    private Vector2 startPos;
    private Vector2 endPos;

    public List<GameObject> SelectedObjects { get; private set; } = new List<GameObject>();
    private List<GameObject> previewSelection = new List<GameObject>();

    void Update()
    {
        if (!IsSelectionEnabled) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            selectionBoxUI.gameObject.SetActive(true);
        }

        if (Input.GetMouseButton(0))
        {
            endPos = Input.mousePosition;
            DrawSelectionBox();
            PreviewSelection();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ConfirmSelection();
            selectionBoxUI.gameObject.SetActive(false);
        }
    }

    void DrawSelectionBox()
    {
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
            if (hit.CompareTag("Selectable"))
            {
                previewSelection.Add(hit.gameObject);

                var sr = hit.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.cyan;
            }
        }
    }

    void ConfirmSelection()
    {
        foreach (var obj in SelectedObjects)
        {
            if (obj != null)
            {
                var sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.white;
            }
        }
        SelectedObjects.Clear();

        foreach (var obj in previewSelection)
        {
            if (obj != null)
            {
                SelectedObjects.Add(obj);

                var sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.yellow;
            }
        }

        previewSelection.Clear();

        Debug.Log("Selected " + SelectedObjects.Count + " objects");
    }
}
