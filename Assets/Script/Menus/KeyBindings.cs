using UnityEngine;

public static class KeyBindings
{
    public static KeyCode UndoKey = KeyCode.Z;
    public static KeyCode CutKey = KeyCode.X;
    public static KeyCode DuplicateKey = KeyCode.C;
    public static KeyCode ConfirmKey = KeyCode.V;

    // 🔥 Helper function buat cek tombol (support keyboard & mouse)
    public static bool GetKeyDown(KeyCode key)
    {
        if (key == KeyCode.Mouse0) return Input.GetMouseButtonDown(0);
        if (key == KeyCode.Mouse1) return Input.GetMouseButtonDown(1);
        return Input.GetKeyDown(key);
    }
}
