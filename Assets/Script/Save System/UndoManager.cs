using UnityEngine;
using UnityEngine.SceneManagement;

public class UndoManager : MonoBehaviour
{
    [Header("Undo Settings")]
    [Tooltip("Tombol default untuk undo jika belum ada di Settings.")]
    public KeyCode defaultUndoKey = KeyCode.Z;

    private void Update()
    {
        KeyCode currentUndoKey = KeyBindings.UndoKey != KeyCode.None ? KeyBindings.UndoKey : defaultUndoKey;

        if (Input.GetKeyDown(currentUndoKey))
        {
            PerformUndo();
        }
    }

    private void PerformUndo()
    {
        if (SaveSystem.Instance != null)
        {
            Debug.Log("↩️ Undo pressed! Restoring last save...");

            var data = SaveSystem.Instance.Load();
            if (data != null)
            {
                // Reload scene → RestoreSave() dipanggil otomatis lewat GameModeManager.Start()
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                Debug.LogWarning("⚠️ Tidak ada save untuk di-undo!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ SaveSystem instance not found!");
        }
    }
}
