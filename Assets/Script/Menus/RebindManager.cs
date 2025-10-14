using UnityEngine;
using UnityEngine.InputSystem;
using System;

public static class RebindManager
{
    public static void StartRebind(InputAction action, int bindingIndex, Action onComplete)
    {
        if (action == null)
        {
            Debug.LogError("Cannot rebind a null InputAction");
            return;
        }

        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
        {
            Debug.LogError("Invalid binding index");
            return;
        }

        var binding = action.bindings[bindingIndex];

        // Don’t allow rebinding the composite itself
        if (binding.isComposite)
        {
            Debug.LogWarning("Cannot rebind a composite directly. Must rebind its parts.");
            return;
        }

        // Perform Interactive Rebind
        action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithCancelingThrough("<Keyboard>/escape") // ESC Cancels
            .OnComplete(operation =>
            {
                operation.Dispose();

                SaveRebinds(action);

                onComplete?.Invoke();
            })
            .Start();
    }

    private static void SaveRebinds(InputAction action)
    {
        string json = action.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(action.actionMap.name + "." + action.name, json);
        PlayerPrefs.Save();
    }

    public static void LoadRebinds(InputAction action)
    {
        string key = action.actionMap.name + "." + action.name;
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            action.LoadBindingOverridesFromJson(json);
        }
    }
}
