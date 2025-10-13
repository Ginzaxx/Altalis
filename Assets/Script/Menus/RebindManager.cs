using UnityEngine;
using UnityEngine.InputSystem;
using System;

public static class RebindManager
{
    public static void StartRebind(InputAction action, string scheme, Action onComplete)
    {
        if (action == null)
        {
            Debug.LogError("Cannot rebind a null InputAction");
            return;
        }

        // Find the binding index for the control scheme
        int bindingIndex = -1;
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (action.bindings[i].groups.Contains(scheme))
            {
                bindingIndex = i;
                break;
            }
        }

        if (bindingIndex == -1)
        {
            Debug.LogWarning($"No binding found for {action.name} in {scheme}");
            return;
        }

        // Perform rebinding
        action.PerformInteractiveRebinding(bindingIndex)
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
