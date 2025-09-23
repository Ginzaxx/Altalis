using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingMenu : MonoBehaviour
{
    [Header("References")]

    [Header("UI Buttons")]
    [SerializeField] private Button undoKeyButton;
    [SerializeField] private TMP_Text undoKeyLabel;

    [SerializeField] private Button cutKeyButton;
    [SerializeField] private TMP_Text cutKeyLabel;

    [SerializeField] private Button duplicateKeyButton;
    [SerializeField] private TMP_Text duplicateKeyLabel;

    [SerializeField] private Button confirmKeyButton;
    [SerializeField] private TMP_Text confirmKeyLabel;

    private string waitingForKey = null;

    void Start()
    {
        // Load saved keys (default jika belum ada)
        KeyBindings.UndoKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("UndoKey", "Z"));
        KeyBindings.CutKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("CutKey", "X"));
        KeyBindings.DuplicateKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("DuplicateKey", "C"));
        KeyBindings.ConfirmKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ConfirmKey", "V"));

        UpdateLabels();

        // Tambahkan listener ke tombol
        undoKeyButton.onClick.AddListener(() => StartRebind("Undo"));
        cutKeyButton.onClick.AddListener(() => StartRebind("Cut"));
        duplicateKeyButton.onClick.AddListener(() => StartRebind("Duplicate"));
        confirmKeyButton.onClick.AddListener(() => StartRebind("Confirm"));
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(waitingForKey))
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(k))
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

    private void StartRebind(string action)
    {
        waitingForKey = action;
        Debug.Log("Press a new key for " + action);
    }

    private void ApplyRebind(string action, KeyCode newKey)
    {
        switch (action)
        {
            case "Undo":
                KeyBindings.UndoKey = newKey;
                PlayerPrefs.SetString("UndoKey", newKey.ToString());
                break;
            case "Cut":
                KeyBindings.CutKey = newKey;
                PlayerPrefs.SetString("CutKey", newKey.ToString());
                break;
            case "Duplicate":
                KeyBindings.DuplicateKey = newKey;
                PlayerPrefs.SetString("DuplicateKey", newKey.ToString());
                break;
            case "Confirm":
                KeyBindings.ConfirmKey = newKey;
                PlayerPrefs.SetString("ConfirmKey", newKey.ToString());
                break;
        }
        PlayerPrefs.Save();
    }

    private void UpdateLabels()
    {
        undoKeyLabel.text = KeyBindings.UndoKey.ToString();
        cutKeyLabel.text = KeyBindings.CutKey.ToString();
        duplicateKeyLabel.text = KeyBindings.DuplicateKey.ToString();
        confirmKeyLabel.text = KeyBindings.ConfirmKey.ToString();
    }
}
