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
                Debug.Log("↩️ Undo pressed! Restoring from last ManaBlock...");
                ResourceManager.Instance?.FullRestoreMana();
                SaveSystem.Instance.RestoreLastManaBlock();
            }
            else
            {
                Debug.LogWarning("⚠️ Save System instance not found!");
            }
        }
    }
}
