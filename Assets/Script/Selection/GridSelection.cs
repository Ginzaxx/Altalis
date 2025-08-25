using UnityEngine;
using System.Collections.Generic;

public class DragSelection : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private RectTransform selectionBoxUI; // UI overlay box

    private Vector2 startPos;
    private Vector2 endPos;

    private List<GameObject> selectedObjects = new List<GameObject>();   // final selection
    private List<GameObject> previewObjects = new List<GameObject>();    // preview selection

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            selectionBoxUI.gameObject.SetActive(true);
        }

        if (Input.GetMouseButton(0))
        {
            endPos = Input.mousePosition;
            DrawSelectionBox();
            PreviewSelection(); // tampilkan preview warna saat drag
        }

        if (Input.GetMouseButtonUp(0))
        {
            ConfirmSelection(); // jadikan final selection
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
        // Reset warna preview lama
        foreach (var obj in previewObjects)
        {
            if (obj != null && !selectedObjects.Contains(obj)) // jangan reset kalau sudah final selected
            {
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.white;
            }
        }
        previewObjects.Clear();

        // Buat rect world dari mouse drag
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
                previewObjects.Add(hit.gameObject);

                // Highlight biru untuk preview
                SpriteRenderer sr = hit.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.cyan;
            }
        }
    }

    void ConfirmSelection()
    {
        // Reset warna final selection lama
        foreach (var obj in selectedObjects)
        {
            if (obj != null)
            {
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.white;
            }
        }
        selectedObjects.Clear();

        // Jadikan preview -> final selection
        foreach (var obj in previewObjects)
        {
            if (obj != null)
            {
                selectedObjects.Add(obj);

                // Warna kuning untuk final
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.yellow;
            }
        }

        previewObjects.Clear();

        Debug.Log("Selected " + selectedObjects.Count + " objects");
    }
}
