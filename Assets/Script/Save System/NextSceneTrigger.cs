using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NextSceneTrigger : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("Scene tujuan (drag & drop dari Project).")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;
#endif
    [SerializeField, HideInInspector] private string nextSceneName;

    [Tooltip("Delay sebelum pindah scene")]
    public float delayBeforeLoad = 2f;

    [Header("Fade Effect (optional)")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1f;

    private bool isTriggered = false;

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (sceneAsset != null)
        {
            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            nextSceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        }
#endif
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTriggered && other.CompareTag("Player"))
        {
            isTriggered = true;
            StartCoroutine(LoadNextSceneWithDelay());
        }
    }

    private IEnumerator LoadNextSceneWithDelay()
    {
        // Optional: fade out dulu
        if (fadeCanvas != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeCanvas.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
                yield return null;
            }
            fadeCanvas.alpha = 1f;
        }

        // Tunggu delay
        yield return new WaitForSeconds(delayBeforeLoad);

        // Pindah ke scene yang dipilih
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ Scene tujuan belum dipilih di inspector!");
        }
    }
}
