using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NextSceneTrigger : MonoBehaviour
{
    [Header("Transition Settings")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;
#endif
    [SerializeField, HideInInspector] private string nextSceneName;

    [Tooltip("ID unik portal ini (misalnya 'Door_A')")]
    public string portalID;

    [Tooltip("ID portal tujuan di scene berikutnya (misalnya 'Door_B')")]
    public string targetPortalID;

    [Tooltip("Delay sebelum pindah scene")]
    public float delayBeforeLoad = 1f;

    [Header("Fade Effect (optional)")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 0.5f;

    // 🔒 Cooldown management
    private static float cooldownEndTime = 0f;
    private static float cooldownDuration = 1.5f;
    private static bool IsOnCooldown => Time.time < cooldownEndTime;
    private static void StartCooldown() => cooldownEndTime = Time.time + cooldownDuration;

    private bool isTriggered = false;
    public static string LastUsedPortalID; // portal asal sementara

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
        // 🧠 Cegah trigger langsung setelah spawn
        if (IsOnCooldown) return;

        if (!isTriggered && other.CompareTag("Player"))
        {
            isTriggered = true;
            StartCoroutine(LoadNextSceneWithDelay(other.gameObject));
        }
    }

    private IEnumerator LoadNextSceneWithDelay(GameObject player)
    {
        // 🔹 Simpan target portal sebelum pindah
        LastUsedPortalID = targetPortalID;
        PlayerPrefs.SetString("SpawnFromPortal", targetPortalID);
        PlayerPrefs.Save();

        // 🔹 Optional: fade out
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

        // 🔹 Tunggu sedikit sebelum load
        yield return new WaitForSeconds(delayBeforeLoad);

        // 💾 SAVE sebelum ganti scene
        if (SaveSystem.Instance != null)
        {
            var resourceManager = ResourceManager.Instance;
            int mana = (resourceManager != null) ? resourceManager.CurrentMana : 0;

            SaveSystem.Instance.Save(player.transform.position, mana);
            Debug.Log($"💾 Auto-saved before switching to scene '{nextSceneName}' via portal '{portalID}'");
        }
        else
        {
            Debug.LogWarning("⚠️ SaveSystem.Instance not found! Scene change without saving.");
        }

        // 🔹 Load scene berikutnya
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ Scene tujuan belum dipilih di inspector!");
        }
    }

    // ✅ Dipanggil otomatis oleh SceneSpawnManager saat scene baru dimuat
    public static void SetPlayerSpawnPoint(GameObject player)
    {
        string fromPortal = PlayerPrefs.GetString("SpawnFromPortal", string.Empty);
        if (string.IsNullOrEmpty(fromPortal)) return;

        NextSceneTrigger[] triggers = FindObjectsOfType<NextSceneTrigger>();
        foreach (var trigger in triggers)
        {
            if (trigger.portalID == fromPortal)
            {
                player.transform.position = trigger.transform.position;
                Debug.Log($"🚪 Player spawned at portal '{fromPortal}'");
                break;
            }
        }

        // 🔒 Aktifkan cooldown agar tidak langsung trigger balik
        StartCooldown();

        PlayerPrefs.DeleteKey("SpawnFromPortal");
        PlayerPrefs.Save();
    }
}
