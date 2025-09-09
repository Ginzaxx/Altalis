using UnityEngine;
using UnityEngine.SceneManagement;

public class UndoManager : MonoBehaviour
{
    [Header("Undo Settings")]
    [Tooltip("Tombol untuk undo (default: Z)")]
    public KeyCode undoKey = KeyCode.Z;

    private void Update()
    {
        if (Input.GetKeyDown(undoKey))
        {
            PerformUndo();
        }
    }

    private void PerformUndo()
    {
        if (SaveSystem.Instance != null)
        {
            Debug.Log("↩️ Undo pressed! Restoring last save...");

            // Pastikan data ada sebelum reload scene
            var data = SaveSystem.Instance.Load();
            if (data != null)
            {
                // Reload scene aktif → RestoreSave() akan dipanggil otomatis 
                // dari GameModeManager.Start() atau bisa dipanggil manual.
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
