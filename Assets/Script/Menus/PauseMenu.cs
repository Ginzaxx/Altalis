using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuUI;
    public GameObject settingPanelUI; // Tambahan

    [Header("Buttons")]
    public Button pauseButton;
    public Button resumeButton;
    public Button settingButton;
    public Button quitButton;

    private bool isPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        settingPanelUI.SetActive(false); // pastikan awalnya mati

        resumeButton.onClick.AddListener(Resume);
        settingButton.onClick.AddListener(OpenSetting);
        quitButton.onClick.AddListener(QuitGame);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Kalau lagi di setting panel, balik ke pause menu
            if (settingPanelUI.activeSelf)
            {
                CloseSetting();
            }
            else
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        settingPanelUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void OpenSetting()
    {
        settingPanelUI.SetActive(true);
    }

    public void CloseSetting()
    {
        settingPanelUI.SetActive(false);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; // biar game normal lagi pas keluar
        SceneManager.LoadScene(0);
    }
}
