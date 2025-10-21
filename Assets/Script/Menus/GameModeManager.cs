using UnityEngine;

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
    public DragSelection dragSelectionScript;
    
    [Header("UI Indicators")]
    public GameObject movementModeUI;
    public GameObject selectionModeUI;
    
    [Header("Slow Motion Settings")]
    [Range(0.01f, 1f)]
    public float slowMotionTimeScale = 0.3f;
    [Range(0.01f, 5f)]
    public float transitionSpeed = 5f;
    
    private float targetTimeScale = 1f;
    private float normalTimeScale = 1f;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        SwitchMode(currentMode);

        // Restore + ambil datanya sekali saja
        SaveData data = SaveSystem.Instance.RestoreSave();

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
        {
            NextSceneTrigger.SetPlayerSpawnPoint(player);
        }
    }
    
    void Update()
    {
        // Check for right mouse click to toggle modes
        if (Input.GetMouseButtonDown(1))
        {
            ToggleMode();
        }
        
        // Smooth time scale transition
        if (Mathf.Abs(Time.timeScale - targetTimeScale) > 0.01f)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, transitionSpeed * Time.unscaledDeltaTime);
        }
        else
        {
            Time.timeScale = targetTimeScale;
        }
    }
    
    public void ToggleMode()
    {
        currentMode = (currentMode == GameMode.Movement) ? GameMode.Selection : GameMode.Movement;
        SwitchMode(currentMode);
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
    
    void EnableMovementMode()
    {
        if (movementScript != null)
        {
            movementScript.enabled = true;
        }

        if (dragSelectionScript != null)
        {
            dragSelectionScript.enabled = false;
            dragSelectionScript.IsSelectionEnabled = false;

            // 🔥 Clear selection saat keluar dari Selection Mode
            dragSelectionScript.ClearSelection();

            if (dragSelectionScript.selectionBoxUI != null)
            {
                dragSelectionScript.selectionBoxUI.gameObject.SetActive(false);
            }
        }

        targetTimeScale = normalTimeScale;
    }
    
    void EnableSelectionMode()
    {
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        if (dragSelectionScript != null)
        {
            dragSelectionScript.enabled = true;
            dragSelectionScript.IsSelectionEnabled = true;

            // 🔥 Clear selection saat masuk Selection Mode baru
            dragSelectionScript.ClearSelection();
        }

        targetTimeScale = slowMotionTimeScale;
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
        {
            targetTimeScale = slowMotionTimeScale;
        }
    }
    
    public void SetTransitionSpeed(float newSpeed)
    {
        transitionSpeed = Mathf.Clamp(newSpeed, 0.01f, 5f);
    }
    
    // Emergency method to reset time scale (useful for debugging)
    [System.Obsolete("Use only for debugging purposes")]
    public void ResetTimeScale()
    {
        Time.timeScale = 1f;
        targetTimeScale = 1f;
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
        {
            Time.timeScale = normalTimeScale;
        }
    }
}