using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UndoManager : MonoBehaviour
{
    public void PerformUndo(InputAction.CallbackContext context)
    {
        if (context.performed)
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
                    Debug.LogWarning("⚠️ No Saves to Undo!");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Save System instance not found!");
            }
        }
    }
}
