using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingMenu : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button undoKeyButton;
    [SerializeField] private TMP_Text undoKeyLabel;

    [SerializeField] private Button cutKeyButton;
    [SerializeField] private TMP_Text cutKeyLabel;

    [SerializeField] private Button duplicateKeyButton;
    [SerializeField] private TMP_Text duplicateKeyLabel;

    [SerializeField] private Button confirmKeyButton;
    [SerializeField] private TMP_Text confirmKeyLabel;

    [SerializeField] private Button moveLeftKeyButton;
    [SerializeField] private TMP_Text moveLeftKeyLabel;

    [SerializeField] private Button moveRightKeyButton;
    [SerializeField] private TMP_Text moveRightKeyLabel;

    [SerializeField] private Button jumpKeyButton;
    [SerializeField] private TMP_Text jumpKeyLabel;

    [SerializeField] private Button cameraUpKeyButton;
    [SerializeField] private TMP_Text cameraUpKeyLabel;

    [SerializeField] private Button cameraDownKeyButton;
    [SerializeField] private TMP_Text cameraDownKeyLabel;

    [Header("Panel Control")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeButton;

    [Header("Volume Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider musicSlider;

    [SerializeField] private TMP_Text masterValueLabel;
    [SerializeField] private TMP_Text soundValueLabel;
    [SerializeField] private TMP_Text musicValueLabel;

    private string waitingForKey = null;

    void Start()
    {
        // 🎮 Load keybindings
        KeyBindings.UndoKey        = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("UndoKey", "Z"));
        KeyBindings.CutKey         = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("CutKey", "X"));
        KeyBindings.DuplicateKey   = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("DuplicateKey", "C"));
        KeyBindings.ConfirmKey     = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ConfirmKey", "V"));
        KeyBindings.MoveLeftKey    = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("MoveLeftKey", "A"));
        KeyBindings.MoveRightKey   = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("MoveRightKey", "D"));
        KeyBindings.JumpKey        = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("JumpKey", "Space"));
        KeyBindings.CameraUpKey    = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("CameraUpKey", "W"));
        KeyBindings.CameraDownKey  = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("CameraDownKey", "S"));

        UpdateLabels();

        // 🔊 Load volume preferences
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float soundVol = PlayerPrefs.GetFloat("SoundVolume", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);

        // 🎚️ Setup sliders
        if (masterSlider != null)
        {
            masterSlider.value = masterVol;
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            OnMasterVolumeChanged(masterVol);
        }

        if (soundSlider != null)
        {
            soundSlider.value = soundVol;
            soundSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
            OnSoundVolumeChanged(soundVol);
        }

        if (musicSlider != null)
        {
            musicSlider.value = musicVol;
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            OnMusicVolumeChanged(musicVol);
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(waitingForKey))
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(k) && !IsMouseKey(k))
                    {
                        ApplyRebind(waitingForKey, k);
                        waitingForKey = null;
                        UpdateLabels();
                        break;
                    }
                }
            }
        }
    }

    // =============================
    // 🔑 Key Binding Section
    // =============================

    private void StartRebind(string action)
    {
        waitingForKey = action;
        Debug.Log("Press a new key for " + action);
    }

    private void ApplyRebind(string action, KeyCode newKey)
    {
        switch (action)
        {
            case "Undo":        KeyBindings.UndoKey = newKey; PlayerPrefs.SetString("UndoKey", newKey.ToString()); break;
            case "Cut":         KeyBindings.CutKey = newKey; PlayerPrefs.SetString("CutKey", newKey.ToString()); break;
            case "Duplicate":   KeyBindings.DuplicateKey = newKey; PlayerPrefs.SetString("DuplicateKey", newKey.ToString()); break;
            case "Confirm":     KeyBindings.ConfirmKey = newKey; PlayerPrefs.SetString("ConfirmKey", newKey.ToString()); break;
            case "MoveLeft":    KeyBindings.MoveLeftKey = newKey; PlayerPrefs.SetString("MoveLeftKey", newKey.ToString()); break;
            case "MoveRight":   KeyBindings.MoveRightKey = newKey; PlayerPrefs.SetString("MoveRightKey", newKey.ToString()); break;
            case "Jump":        KeyBindings.JumpKey = newKey; PlayerPrefs.SetString("JumpKey", newKey.ToString()); break;
            case "CameraUp":    KeyBindings.CameraUpKey = newKey; PlayerPrefs.SetString("CameraUpKey", newKey.ToString()); break;
            case "CameraDown":  KeyBindings.CameraDownKey = newKey; PlayerPrefs.SetString("CameraDownKey", newKey.ToString()); break;
        }
        PlayerPrefs.Save();
    }

    private void UpdateLabels()
    {
        undoKeyLabel.text       = KeyBindings.UndoKey.ToString();
        cutKeyLabel.text        = KeyBindings.CutKey.ToString();
        duplicateKeyLabel.text  = KeyBindings.DuplicateKey.ToString();
        confirmKeyLabel.text    = KeyBindings.ConfirmKey.ToString();
        moveLeftKeyLabel.text   = KeyBindings.MoveLeftKey.ToString();
        moveRightKeyLabel.text  = KeyBindings.MoveRightKey.ToString();
        jumpKeyLabel.text       = KeyBindings.JumpKey.ToString();
        cameraUpKeyLabel.text   = KeyBindings.CameraUpKey.ToString();
        cameraDownKeyLabel.text = KeyBindings.CameraDownKey.ToString();
    }

    private bool IsMouseKey(KeyCode key)
    {
        return key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6;
    }

    private void ClosePanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // =============================
    // 🔊 Volume Section
    // =============================

    private void OnMasterVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();

        if (masterValueLabel != null)
            masterValueLabel.text = Mathf.RoundToInt(value * 100).ToString();

        ApplyVolumeSettings();
    }

    private void OnSoundVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SoundVolume", value);
        PlayerPrefs.Save();

        if (soundValueLabel != null)
            soundValueLabel.text = Mathf.RoundToInt(value * 100).ToString();

        ApplyVolumeSettings();
    }

    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();

        if (musicValueLabel != null)
            musicValueLabel.text = Mathf.RoundToInt(value * 100).ToString();

        ApplyVolumeSettings();
    }

    private void ApplyVolumeSettings()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float sound = PlayerPrefs.GetFloat("SoundVolume", 1f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);

        // 🎧 Final volume output
        float finalSFX = master * sound;
        float finalAmbience = master * sound; // ambience ikut sound
        float finalBGM = master * music;

        SoundManager.SetSFXVolume(finalSFX);
        SoundManager.SetAmbienceVolume(finalAmbience);
        SoundManager.SetBGMVolume(finalBGM);
    }
}
