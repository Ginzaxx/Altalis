using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StartMenuController : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject creditsCanvas;
    [SerializeField] private CanvasGroup blackScreenGroup;

    [Header("Duration")]
    public float fadeDuration = 1f;

    [Header("Scene to Load")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;
#endif
    [SerializeField, HideInInspector] private string sceneName;

    private bool isFading = false;

    void OnValidate()
    {
#if UNITY_EDITOR
        if (sceneAsset != null)
        {
            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        }
#endif
    }

    void Start()
    {
        creditsCanvas.SetActive(false);
        blackScreenGroup.alpha = 0f;
    }

    IEnumerator FadeInAndLoadScene()
    {
        isFading = true;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            blackScreenGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        blackScreenGroup.alpha = 1f;
        isFading = false;

        SceneManager.LoadScene(sceneName);
    }

    // === UI BUTTONS ===

    public void OnContinueClicked()
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.Load() != null)
        {
            // ✅ Ada save → langsung load
            FadeIn();
        }
        else
        {
            // ❌ Tidak ada save → treat as New Game
            Debug.LogWarning("⚠️ No save found! Starting a new game.");
            OnNewGameClicked();
        }
    }

    public void OnNewGameClicked()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.DeleteSave(); // 🗑️ Delete save lama
        }

        FadeIn();
    }

    public void OnCreditsClicked()
    {
        creditsCanvas.SetActive(!creditsCanvas.activeSelf);
    }

    public void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // === Fade Helper ===
    public void FadeIn()
    {
        if (!isFading)
            StartCoroutine(FadeInAndLoadScene());
    }
}
