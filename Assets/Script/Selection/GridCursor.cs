using UnityEngine;
using UnityEngine.EventSystems;

public class GridCursor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;   // drag Main Camera ke sini
    [SerializeField] public Grid grid;    // drag object 'Grid' ke sini

    [Header("Options")]
    [SerializeField] private bool lockZ = true;
    [SerializeField] private float zDepth = 0f; // taruh di 0 atau layer khusus

    public Vector3Int CurrentCell { get; private set; }
    public Vector3 CurrentCellCenter { get; private set; }

    void Reset()
    {
        cam = Camera.main;
        grid = FindObjectOfType<Grid>();
    }

    void Update()
    {
        // (Opsional) jangan gerak kalau mouse lagi di atas UI
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;
        if (cam == null || grid == null) return;

        // 1) Screen → World
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        // 2) World → Cell
        CurrentCell = grid.WorldToCell(mouseWorld);

        // 3) Cell → World (pusat sel)
        CurrentCellCenter = grid.GetCellCenterWorld(CurrentCell);

        // 4) Pindahkan cursor ke pusat sel
        Vector3 target = CurrentCellCenter;
        if (lockZ) target.z = zDepth;
        else target.z = transform.position.z;

        transform.position = target;
    }

    // (Opsional) Biar kelihatan di editor posisi cell
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.92f, 0.16f, 0.6f);
        Gizmos.DrawWireCube(CurrentCellCenter, new Vector3(grid != null ? grid.cellSize.x : 1f, grid != null ? grid.cellSize.y : 1f, 0.01f));
    }
}
