using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Button pauseButton;
    public Button resumeButton;
    public Button settingButton;
    public Button quitButton;

    private bool isPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(false);

        resumeButton.onClick.AddListener(Resume);
        settingButton.onClick.AddListener(Setting);
        quitButton.onClick.AddListener(QuitGame);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
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
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Setting()
    {

    }

    public void QuitGame()
    {
        SceneManager.LoadScene(0);
    }
}
