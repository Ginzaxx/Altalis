using UnityEngine;
using UnityEngine.InputSystem;

public enum GameMode
{
    Movement,
    Selection
}

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }
    
    [Header("Game Mode Settings")]
    public GameMode currentMode = GameMode.Movement;
    
    [Header("References")]
    public Movement movementScript;
    public GridSelection gridSelection;
    public GridCursor gridCursor;
    public GameObject cursor;
    public GameObject player;

    [Header("UI Indicators")]
    public GameObject movementModeUI;
    public GameObject selectionModeUI;

    [Header("Slow Motion Settings")]
    [Range(0.01f, 5f)]
    public float transitionSpeed = 5f;
    [Range(0.01f, 1f)]
    public float slowMotionTimeScale = 0.3f;
    
    private float targetTimeScale = 1f;
    private float normalTimeScale = 1f;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        SwitchMode(currentMode);

        // Restore + ambil datanya sekali saja
        SaveData data = SaveSystem.Instance.RestoreSave();
        SaveSystem.Instance.LoadPlacedObjectsOnly();

        // apply posisi player
        if (data != null && movementScript != null)
        {
            movementScript.transform.position = new Vector3(
                data.playerX,
                data.playerY,
                movementScript.transform.position.z
            );
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            NextSceneTrigger.SetPlayerSpawnPoint(player);
    }

    void Update()
    {
        // Smooth time scale transition
        if (Mathf.Abs(Time.timeScale - targetTimeScale) > 0.01f)
            Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, transitionSpeed * Time.unscaledDeltaTime);
        else
            Time.timeScale = targetTimeScale;
    }
    
    public void ToggleMode(InputAction.CallbackContext context)
    {
        // ✅ Pastikan script aktif dulu
        if (!enabled) 
            return;

        if (context.performed)
        {
            currentMode = (currentMode == GameMode.Movement) ? GameMode.Selection : GameMode.Movement;
            SwitchMode(currentMode);
            gridSelection.ClearSelection();
        }
    }

    public void SwitchMode(GameMode newMode)
    {
        currentMode = newMode;

        switch (currentMode)
        {
            case GameMode.Movement:
                EnableMovementMode();
                break;
            case GameMode.Selection:
                EnableSelectionMode();
                break;
        }

        UpdateUI();
        Debug.Log("Switched to " + currentMode + " Mode");
    }
    
    void UpdateUI()
    {
        // Update UI indicators
        if (movementModeUI != null)
            movementModeUI.SetActive(currentMode == GameMode.Movement);

        if (selectionModeUI != null)
            selectionModeUI.SetActive(currentMode == GameMode.Selection);
    }

    // Public getter for other scripts to check current mode
    void EnableMovementMode()
    {
        SoundManager.PlaySound("ManaOff", 1);
        
        if (movementScript != null)
            movementScript.enabled = true;

        if (gridCursor != null)
            gridCursor.gameObject.SetActive(false);

        if (gridSelection != null)
        {
            gridSelection.enabled = false;
            gridSelection.IsSelectionEnabled = false;

            // Clear selection when exiting Selection Mode
            gridSelection.ClearSelection();

            if (gridSelection.selectionBoxUI != null)
                gridSelection.selectionBoxUI.gameObject.SetActive(false);

        }

        targetTimeScale = normalTimeScale;
    }
    
    void EnableSelectionMode()
    {
        SoundManager.PlaySound("ManaOn", 1);

        if (movementScript != null)
            movementScript.enabled = false;

        if (gridCursor != null)
        {
            gridCursor.gameObject.SetActive(true);
            cursor.transform.position = player.transform.position;
        }

        if (gridSelection != null)
        {
            gridSelection.enabled = true;
            gridSelection.IsSelectionEnabled = true;

            // Clear selection when entering Selection Mode
            gridSelection.ClearSelection();
        }

        targetTimeScale = slowMotionTimeScale;
    }

    public bool IsMovementMode()
    {
        return currentMode == GameMode.Movement;
    }

    public bool IsSelectionMode()
    {
        return currentMode == GameMode.Selection;
    }

    // Method to set custom time scales
    public void SetSlowMotionTimeScale(float newTimeScale)
    {
        slowMotionTimeScale = Mathf.Clamp(newTimeScale, 0.01f, 1f);
        if (currentMode == GameMode.Selection)
            targetTimeScale = slowMotionTimeScale;
    }

    public void SetTransitionSpeed(float newSpeed)
    {
        transitionSpeed = Mathf.Clamp(newSpeed, 0.01f, 5f);
    }

    // Emergency method to reset time scale (useful for debugging)
    [System.Obsolete("Use only for debugging purposes")]
    public void ResetTimeScale()
    {
        targetTimeScale = 1f;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    void OnDestroy()
    {
        // Reset time scale when object is destroyed
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Reset time scale when application is paused/unpaused
        if (!pauseStatus && currentMode == GameMode.Movement)
            Time.timeScale = normalTimeScale;
    }
}