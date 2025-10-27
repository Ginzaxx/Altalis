using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GridCursor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] public Grid grid;

    [Header("Options")]
    [SerializeField] private bool LockZ = true;
    [SerializeField] private float ZDepth = 0f;
    [SerializeField] private float MoveSpeed = 8f;
    [SerializeField] private float StickDeadzone = 0.1f;

    public Vector3Int CurrentCell { get; private set; }
    public Vector3 CurrentCellCenter { get; private set; }

    private Vector3 VirtualCursorPos;   // Gamepad Cursor Position
    private Vector3 WorldPos;           // General Cursor Position
    private Vector2 MoveInput;          // Input System
    private bool UseGamepadCursor = false;


    void Reset()
    {
        cam = Camera.main;
        grid = FindObjectOfType<Grid>();
    }

    // 🔹 Called by Input System when using stick or arrow keys
    public void OnMoveCursor(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();

        // When input detected, enable gamepad mode
        if (MoveInput.sqrMagnitude > StickDeadzone * StickDeadzone)
            UseGamepadCursor = true;
    }

    void Update()
    {
        // (Opsional) jangan gerak kalau mouse lagi di atas UI
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;
        if (cam == null || grid == null) return;

        // Check if Mouse Moved → switch back to Mouse Mode
        if (Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero)
            UseGamepadCursor = false;

        // 1) Screen → World
        if (!UseGamepadCursor)
        {
            // Mouse mode
            WorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            VirtualCursorPos = WorldPos; // Sync so both stay aligned
        }
        else
        {
            // Gamepad mode
            VirtualCursorPos += new Vector3(MoveInput.x, MoveInput.y, 0) * MoveSpeed * Time.deltaTime;

            // Optional: clamp position to camera bounds
            Vector3 min = cam.ViewportToWorldPoint(Vector3.zero);
            Vector3 max = cam.ViewportToWorldPoint(Vector3.one);
            VirtualCursorPos.x = Mathf.Clamp(VirtualCursorPos.x, min.x, max.x);
            VirtualCursorPos.y = Mathf.Clamp(VirtualCursorPos.y, min.y, max.y);

            WorldPos = VirtualCursorPos;
        }

        // 2) World → Cell → (Cell Center)
        CurrentCell = grid.WorldToCell(WorldPos);
        CurrentCellCenter = grid.GetCellCenterWorld(CurrentCell);

        // 1) Move Cursor to Cell Center
        Vector3 target = CurrentCellCenter;
        if (LockZ) target.z = ZDepth;
        else target.z = transform.position.z;

        transform.position = target;
    }

    // (Opsional) Biar kelihatan di editor posisi cell
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.92f, 0.16f, 0.6f);
        Gizmos.DrawWireCube(CurrentCellCenter, new Vector3(grid.cellSize.x, grid.cellSize.y, 0.01f));
    }
}
