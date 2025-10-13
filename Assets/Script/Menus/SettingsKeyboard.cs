using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class SettingsKeyboard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Keyboard Labels")]
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
    [SerializeField] private GameObject keyboardPanel;
    [SerializeField] private Button closeButton;

    private const string scheme = "Keyboard&Mouse";

    private void Start()
    {
                // Load saved overrides
        foreach (var action in playerInput.actions)
            RebindManager.LoadRebinds(action);

        // Setup listeners
        undoKeyButton.onClick.AddListener(()        => StartRebind("Undo", undoKeyLabel));
        cutKeyButton.onClick.AddListener(()         => StartRebind("Cut", cutKeyLabel));
        copyKeyButton.onClick.AddListener(()        => StartRebind("Copy", copyKeyLabel));
        pasteKeyButton.onClick.AddListener(()       => StartRebind("Paste", pasteKeyLabel));
        
        jumpKeyButton.onClick.AddListener(()        => StartRebind("Jump", jumpKeyLabel));
        moveLeftKeyButton.onClick.AddListener(()    => StartRebind("MoveLeft", moveLeftKeyLabel));
        moveRightKeyButton.onClick.AddListener(()   => StartRebind("MoveRight", moveRightKeyLabel));
        
        cameraUpKeyButton.onClick.AddListener(()    => StartRebind("CameraUp", cameraUpKeyLabel));
        cameraDownKeyButton.onClick.AddListener(()  => StartRebind("CameraDown", cameraDownKeyLabel));

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void OnEnable()
    {
        UpdateLabels();
    }

    public void StartRebind(string actionName, TMP_Text label)
    {
        InputAction action = playerInput.actions[actionName];

        label.text = "Press any Button...";

        RebindManager.StartRebind(action, scheme, () =>
        {
            label.text = action.GetBindingDisplayString();
        });
    }

    private void UpdateLabels()
    {
        undoKeyLabel.text       = GetBindingDisplay("Undo");
        cutKeyLabel.text        = GetBindingDisplay("Cut");
        copyKeyLabel.text       = GetBindingDisplay("Copy");
        pasteKeyLabel.text      = GetBindingDisplay("Paste");
        
        jumpKeyLabel.text       = GetBindingDisplay("Jump");
        moveLeftKeyLabel.text   = GetBindingDisplay("MoveLeft");
        moveRightKeyLabel.text  = GetBindingDisplay("MoveRight");
        
        cameraUpKeyLabel.text   = GetBindingDisplay("CameraUp");
        cameraDownKeyLabel.text = GetBindingDisplay("CameraDown");
    }

    private string GetBindingDisplay(string actionName)
    {
        InputAction action = playerInput.actions[actionName];
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (action.bindings[i].groups.Contains(scheme))
                return action.GetBindingDisplayString(i);
        }
        return "<Not Bound>";
    }

    private void ClosePanel()
    {
        settingsPanel.SetActive(true);
        keyboardPanel.SetActive(false);
    }
}