using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class SettingsGamepad : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Gamepad Labels")]
    [SerializeField] private Button undoKeyButton;
    [SerializeField] private TMP_Text undoKeyLabel;
    [SerializeField] private Button cutKeyButton;
    [SerializeField] private TMP_Text cutKeyLabel;
    [SerializeField] private Button copyKeyButton;
    [SerializeField] private TMP_Text copyKeyLabel;
    [SerializeField] private Button pasteKeyButton;
    [SerializeField] private TMP_Text pasteKeyLabel;

    [SerializeField] private Button jumpKeyButton;
    [SerializeField] private TMP_Text jumpKeyLabel;
    [SerializeField] private Button moveLeftKeyButton;
    [SerializeField] private TMP_Text moveLeftKeyLabel;
    [SerializeField] private Button moveRightKeyButton;
    [SerializeField] private TMP_Text moveRightKeyLabel;

    [SerializeField] private Button cameraUpKeyButton;
    [SerializeField] private TMP_Text cameraUpKeyLabel;
    [SerializeField] private Button cameraDownKeyButton;
    [SerializeField] private TMP_Text cameraDownKeyLabel;

    [Header("Panel Control")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gamepadPanel;
    [SerializeField] private Button closeButton;

    private const string scheme = "Gamepad";

    private void Start()
    {
        // Load saved overrides
        foreach (var map in inputActions.actionMaps)
        {
            foreach (var action in map.actions)
                RebindManager.LoadRebinds(action);
        }

        foreach (var binding in inputActions.FindAction("Camera").bindings)
        {
            Debug.Log($"{binding.name}: groups = '{binding.groups}'");
        }

        // Setup listeners
        undoKeyButton.onClick.AddListener(()        => StartRebind("Undo", null, undoKeyLabel));
        cutKeyButton.onClick.AddListener(()         => StartRebind("Cut", null, cutKeyLabel));
        copyKeyButton.onClick.AddListener(()        => StartRebind("Copy", null, copyKeyLabel));
        pasteKeyButton.onClick.AddListener(()       => StartRebind("Paste", null, pasteKeyLabel));

        jumpKeyButton.onClick.AddListener(()        => StartRebind("Jump", null, jumpKeyLabel));
        moveLeftKeyButton.onClick.AddListener(()    => StartRebind("Move", "Left", moveLeftKeyLabel));
        moveRightKeyButton.onClick.AddListener(()   => StartRebind("Move", "Right", moveRightKeyLabel));

        cameraUpKeyButton.onClick.AddListener(()    => StartRebind("Camera", "Up", cameraUpKeyLabel));
        cameraDownKeyButton.onClick.AddListener(()  => StartRebind("Camera", "Down", cameraDownKeyLabel));

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void OnEnable()
    {
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        undoKeyLabel.text       = GetBindingDisplay("Undo");
        cutKeyLabel.text        = GetBindingDisplay("Cut");
        copyKeyLabel.text       = GetBindingDisplay("Copy");
        pasteKeyLabel.text      = GetBindingDisplay("Paste");
        
        jumpKeyLabel.text       = GetBindingDisplay("Jump");
        moveLeftKeyLabel.text   = GetBindingDisplay("Move", "Left");
        moveRightKeyLabel.text  = GetBindingDisplay("Move", "Right");
        
        cameraUpKeyLabel.text   = GetBindingDisplay("Camera", "Up");
        cameraDownKeyLabel.text = GetBindingDisplay("Camera", "Down");
    }

    public void StartRebind(string actionName, string bindingName, TMP_Text label)
    {
        var action = inputActions.FindAction(actionName, true);
        int index = GetBindingIndex(action, bindingName);

        if (index == -1)
        {
            Debug.LogWarning($"No binding found for {actionName} ({bindingName ?? "default"}) in {scheme}");
            return;
        }

        label.text = "Press any Button...";

        RebindManager.StartRebind(action, index, () =>
        {
            label.text = action.GetBindingDisplayString();
        });
    }

    private int GetBindingIndex(InputAction action, string bindingName = null)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
             var binding = action.bindings[i];

            if (!string.IsNullOrEmpty(binding.groups) && !binding.groups.Contains(scheme)) continue;

            if (string.IsNullOrEmpty(bindingName) && !binding.isPartOfComposite) return i;

            if (!string.IsNullOrEmpty(bindingName) && binding.isPartOfComposite && binding.name == bindingName) return i;
        }
        return -1;
    }

    private string GetBindingDisplay(string actionName, string bindingName = null)
    {
        var action = inputActions.FindAction(actionName, true);
        int index = GetBindingIndex(action, bindingName);
        return index != -1 ? action.GetBindingDisplayString(index) : "<Not Bound>";
    }

    private void ClosePanel()
    {
        SoundManager.PlaySound("MenuActive", 0.8f);
        settingsPanel.SetActive(true);
        gamepadPanel.SetActive(false);
    }
}