using UnityEngine;
using UnityEngine.UI;

public class SettingsKeybinds : MonoBehaviour
{
    
    [Header("Objects")]
    [SerializeField] private GameObject keyboardPanel;
    [SerializeField] private GameObject gamepadPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject pausePanel;
    
    [Header("Buttons")]
    [SerializeField] private Button keyboardButton;
    [SerializeField] private Button gamepadButton;
    [SerializeField] private Button closeButton;

    void Start()
    {
        keyboardButton.onClick.AddListener(ShowKeyboard);
        gamepadButton.onClick.AddListener(ShowGamepad);
        closeButton.onClick.AddListener(Close);
    }

    public void ShowKeyboard()
    {
        keyboardPanel.SetActive(true);
        gamepadPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void ShowGamepad()
    {
        gamepadPanel.SetActive(true);
        keyboardPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    private void Close()
    {
        SoundManager.PlaySound("MenuActive", 0.8f);
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }
}
