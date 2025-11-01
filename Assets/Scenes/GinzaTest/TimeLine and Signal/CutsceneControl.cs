using UnityEngine;

public class CutsceneControl : MonoBehaviour
{
    [Header("Reference to your movement script")]
    public MonoBehaviour playerControlScript; // Drag your Movement script here manually

    public void EnablePlayerControl()
    {
        if (playerControlScript != null)
        {
            playerControlScript.enabled = true;
            Debug.Log("✅ Player control re-enabled!");
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerControlScript reference not set in Inspector!");
        }
    }
}
