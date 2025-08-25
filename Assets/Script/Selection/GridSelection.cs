using UnityEngine;
using System.Collections.Generic;

public class DragSelection : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private RectTransform selectionBoxUI; // UI overlay box

    private Vector2 startPos;
    private Vector2 endPos;

    private List<GameObject> selectedObjects = new List<GameObject>();

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
        }

        if (Input.GetMouseButtonUp(0))
        {
            SelectObjects();
            selectionBoxUI.gameObject.SetActive(false);
        }
    }

    void DrawSelectionBox()
    {
        Canvas parentCanvas = selectionBoxUI.GetComponentInParent<Canvas>();
        RectTransform canvasRect = parentCanvas.transform as RectTransform;

        Vector2 localStartPos;
        Vector2 localEndPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, startPos, parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam, out localStartPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, endPos, parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam, out localEndPos);

        Vector2 boxCenter = (localStartPos + localEndPos) / 2f;
        Vector2 boxSize = new Vector2(Mathf.Abs(localEndPos.x - localStartPos.x), Mathf.Abs(localEndPos.y - localStartPos.y));

        selectionBoxUI.anchoredPosition = boxCenter;
        selectionBoxUI.sizeDelta = boxSize;
    }



    void SelectObjects()
    {
        // Reset warna object lama
        foreach (var obj in selectedObjects)
        {
            if (obj != null)
            {
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.white;
            }
        }
        selectedObjects.Clear();

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
                selectedObjects.Add(hit.gameObject);

                // Highlight kuning
                SpriteRenderer sr = hit.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.yellow;
            }
        }

        Debug.Log("Selected " + selectedObjects.Count + " objects");
    }
}
