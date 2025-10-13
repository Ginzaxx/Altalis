using UnityEngine;
using System.Collections.Generic;

public class DragSelection : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] public RectTransform selectionBoxUI;
    public bool IsSelectionEnabled { get; set; } = true;

    private Vector2 startPos;
    private Vector2 endPos;
    private bool isDragging = false;

    [Header("Slow Motion Settings")]
    public bool useUnscaledTimeForUI = true;

    public List<GameObject> SelectedObjects { get; private set; } = new List<GameObject>();
    private List<GameObject> previewSelection = new List<GameObject>();

    // Limit dari ResourceManager
    private int MaxSelectable => ResourceManager.Instance != null ? ResourceManager.Instance.SelectLimit : 3;

    void Update()
    {
        if (!IsSelectionEnabled || !enabled) return;
        if (Input.GetMouseButtonDown(1)) return; // abaikan klik kanan

        // 🔹 TAP (klik cepat tanpa drag)
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            isDragging = true;

            if (selectionBoxUI != null)
                selectionBoxUI.gameObject.SetActive(false);
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            endPos = Input.mousePosition;

            // Jika jarak drag cukup jauh → drag select
            if (Vector2.Distance(startPos, endPos) > 15f)
            {
                if (selectionBoxUI != null)
                    selectionBoxUI.gameObject.SetActive(true);

                UpdateSelectionBox();
                PreviewSelection();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            // Jika drag kecil (tap)
            if (Vector2.Distance(startPos, Input.mousePosition) < 15f)
                HandleTapSelection();
            else
                ConfirmSelection();

            isDragging = false;

            if (selectionBoxUI != null)
                selectionBoxUI.gameObject.SetActive(false);
        }
    }

    // 🔹 Tap Selection (klik 1 objek tanpa reset)
    void HandleTapSelection()
    {
        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

        if (hit != null)
        {
            GameObject target = hit.gameObject;

            // Naik ke parent jika anak collider
            if (target.transform.parent != null && target.transform.parent.CompareTag("Selectable"))
                target = target.transform.parent.gameObject;

            if (target.CompareTag("Selectable"))
            {
                // Toggle on/off selection
                if (SelectedObjects.Contains(target))
                {
                    DeselectObject(target);
                }
                else
                {
                    if (SelectedObjects.Count < MaxSelectable)
                    {
                        SelectObject(target);
                    }
                    else
                    {
                        Debug.Log($"⚠️ Sudah mencapai batas seleksi: {MaxSelectable}");
                    }
                }

                return; // Jangan reset selection
            }
        }

        // 🔹 Jika klik area kosong → reset semua
        ClearSelection();
    }

    // 🔹 Drag box update
    void UpdateSelectionBox()
    {
        if (selectionBoxUI == null) return;

        Canvas parentCanvas = selectionBoxUI.GetComponentInParent<Canvas>();
        RectTransform canvasRect = parentCanvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, startPos,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            out Vector2 localStartPos
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, endPos,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            out Vector2 localEndPos
        );

        Vector2 boxCenter = (localStartPos + localEndPos) / 2f;
        Vector2 boxSize = new Vector2(Mathf.Abs(localEndPos.x - localStartPos.x), Mathf.Abs(localEndPos.y - localStartPos.y));

        selectionBoxUI.anchoredPosition = boxCenter;
        selectionBoxUI.sizeDelta = boxSize;
    }

    // 🔹 Preview selection saat drag
    void PreviewSelection()
    {
        // Reset warna preview sebelumnya
        foreach (var obj in previewSelection)
        {
            if (obj != null && !SelectedObjects.Contains(obj))
                SetColor(obj, Color.white);
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
            if (target.transform.parent != null && target.transform.parent.CompareTag("Selectable"))
                target = target.transform.parent.gameObject;

            if (target.CompareTag("Selectable") && !previewSelection.Contains(target))
            {
                previewSelection.Add(target);
                SetColor(target, Color.cyan);
            }
        }
    }

    // 🔹 Konfirmasi drag selection
    void ConfirmSelection()
    {
        foreach (var obj in previewSelection)
        {
            if (obj == null) continue;

            if (!SelectedObjects.Contains(obj))
            {
                if (SelectedObjects.Count < MaxSelectable)
                    SelectObject(obj);
            }
        }

        // Reset warna preview
        foreach (var obj in previewSelection)
        {
            if (obj != null && !SelectedObjects.Contains(obj))
                SetColor(obj, Color.white);
        }
        previewSelection.Clear();

        Debug.Log($"Selected {SelectedObjects.Count} objects");
    }

    // 🔹 Tambah ke daftar selection
    void SelectObject(GameObject obj)
    {
        if (!SelectedObjects.Contains(obj))
        {
            SelectedObjects.Add(obj);
            SetColor(obj, Color.yellow);
        }
    }

    // 🔹 Hapus dari daftar selection
    void DeselectObject(GameObject obj)
    {
        if (SelectedObjects.Contains(obj))
        {
            SelectedObjects.Remove(obj);
            SetColor(obj, Color.white);
        }
    }

    // 🔹 Set warna semua renderer anak
    void SetColor(GameObject obj, Color color)
    {
        foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
            sr.color = color;
    }

    // 🔹 Bersihkan semua seleksi
    public void ClearSelection()
    {
        foreach (var obj in SelectedObjects)
        {
            if (obj != null)
                SetColor(obj, Color.white);
        }
        SelectedObjects.Clear();

        foreach (var obj in previewSelection)
        {
            if (obj != null)
                SetColor(obj, Color.white);
        }
        previewSelection.Clear();

        if (selectionBoxUI != null)
            selectionBoxUI.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        ClearSelection();
        isDragging = false;
    }
}
