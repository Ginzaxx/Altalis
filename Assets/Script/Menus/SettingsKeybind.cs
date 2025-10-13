using UnityEngine;
using UnityEngine.UI;

public class SettingsKeybinds : MonoBehaviour
{
    [SerializeField] private GameObject keyboardPanel;
    [SerializeField] private GameObject gamepadPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button keyboardButton;
    [SerializeField] private Button gamepadButton;


    void Start()
    {
        keyboardButton.onClick.AddListener(ShowKeyboard);
        gamepadButton.onClick.AddListener(ShowGamepad);
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
}
