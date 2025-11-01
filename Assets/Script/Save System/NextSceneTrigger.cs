using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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

    [Header("Exit Animation Settings (saat keluar portal)")]
    public MoveDirection moveDirection = MoveDirection.None;
    public float moveDistance = 1.5f;
    public float moveSpeed = 3f;
    public bool jumpInstead = false;
    public float jumpForce = 5f;

    public enum MoveDirection { None, Left, Right, Up, Down }

    // Cooldown management
    private static float cooldownEndTime = 0f;
    private static float cooldownDuration = 1.5f;
    private static bool IsOnCooldown => Time.time < cooldownEndTime;
    private static void StartCooldown() => cooldownEndTime = Time.time + cooldownDuration;

    private bool isTriggered = false;
    public static string LastUsedPortalID;

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
        if (IsOnCooldown) return;

        if (!isTriggered && other.CompareTag("Player"))
        {
            isTriggered = true;
            StartCoroutine(LoadNextSceneWithDelay(other.gameObject));
        }
    }

    private IEnumerator LoadNextSceneWithDelay(GameObject player)
    {
        // Simpan ID portal tujuan
        LastUsedPortalID = targetPortalID;
        PlayerPrefs.SetString("SpawnFromPortal", targetPortalID);
        PlayerPrefs.Save();

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        Animator anim = player.GetComponent<Animator>();
        PlayerInput input = player.GetComponent<PlayerInput>();
        Movement move = player.GetComponent<Movement>();
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();

        // 🔸 Tunggu 1 detik sebelum mematikan physics
        yield return new WaitForSeconds(0.3f);

        // 🔸 Nonaktifkan kontrol & physics agar tidak jatuh
        if (input != null) input.enabled = false;
        if (move != null) move.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.simulated = false; // 🔒 langsung matikan physics
        }

        // 🔸 Jika portal arah ke atas → tarik player ke atas manual (efek disedot)
        if (moveDirection == MoveDirection.Up)
        {
            float liftDuration = 0.6f;
            float liftHeight = 2f;
            Vector3 start = player.transform.position;
            Vector3 end = start + Vector3.up * liftHeight;

            float t = 0f;
            while (t < liftDuration)
            {
                t += Time.deltaTime;
                player.transform.position = Vector3.Lerp(start, end, t / liftDuration);
                yield return null;
            }

            // 🔸 Sembunyikan player setelah tersedot (opsional)
            if (sr != null)
                sr.enabled = false;
        }

        // 🔸 Fade out setelah player berhenti
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

        // 🔸 Tunggu delay tambahan sebelum pindah scene
        yield return new WaitForSeconds(delayBeforeLoad);

        // 🔸 Save data sebelum pindah scene
        if (SaveSystem.Instance != null)
        {
            var resourceManager = ResourceManager.Instance;
            int mana = (resourceManager != null) ? resourceManager.CurrentMana : 0;
            SaveSystem.Instance.Save(player.transform.position, mana);
            Debug.Log($"💾 Auto-saved before switching to scene '{nextSceneName}' via portal '{portalID}'");
        }

        // 🔸 Pindah scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ Scene tujuan belum dipilih di inspector!");
        }
    }

    // Dipanggil otomatis setelah scene baru dimuat
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

                // Jalankan animasi keluar portal
                trigger.StartCoroutine(trigger.PlayPortalExitAnimation(player));
                break;
            }
        }

        StartCooldown();

        PlayerPrefs.DeleteKey("SpawnFromPortal");
        PlayerPrefs.Save();
    }

    // 🎬 Animasi keluar portal + arah hadap yang benar
    private IEnumerator PlayPortalExitAnimation(GameObject player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        Animator anim = player.GetComponent<Animator>();
        PlayerInput input = player.GetComponent<PlayerInput>();
        Movement move = player.GetComponent<Movement>();

        if (rb == null) yield break;

        // 🔸 Nonaktifkan input sementara
        if (input != null)
            input.enabled = false;

        // 🔸 Fade in dari hitam (kalau ada)
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 1f;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeCanvas.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
                yield return null;
            }
            fadeCanvas.alpha = 0f;
        }

        // 🔸 Tentukan arah keluar portal
        Vector2 dir = Vector2.zero;
        switch (moveDirection)
        {
            case MoveDirection.Left: dir = Vector2.left; break;
            case MoveDirection.Right: dir = Vector2.right; break;
            case MoveDirection.Up: dir = Vector2.up; break;
            case MoveDirection.Down: dir = Vector2.down; break;
        }

        // 🔸 Pastikan player menghadap arah benar
        if (move != null && (dir.x > 0 && !move.GetIsFacingRight() || dir.x < 0 && move.GetIsFacingRight()))
        {
            Vector3 scale = player.transform.localScale;
            scale.x *= -1;
            player.transform.localScale = scale;

            var field = typeof(Movement).GetField("IsFacingRight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(move, dir.x > 0);
        }

        // 🔸 Jalankan animasi jalan/lompat keluar portal
        if (jumpInstead && (moveDirection == MoveDirection.Left || moveDirection == MoveDirection.Right))
        {
            // Tentukan arah horizontal
            float horizontalDir = (moveDirection == MoveDirection.Left) ? -1f : 1f;

            // Reset kecepatan sebelumnya agar hasilnya konsisten
            rb.velocity = Vector2.zero;
            yield return null; // Tunggu 1 frame supaya velocity reset

            // 🎯 Gunakan AddForce dengan ForceMode2D.Impulse untuk hasil lebih kuat
            // Ini akan menghasilkan lompatan miring yang jelas ke kiri/kanan
            Vector2 jumpVelocity = new Vector2(horizontalDir * moveSpeed, jumpForce);
            rb.AddForce(jumpVelocity, ForceMode2D.Impulse);

            // Aktifkan animasi lompat
            anim?.SetBool("Jumping", true);

            // Tunggu sampai player mendarat atau waktu maksimal
            yield return new WaitForSeconds(0.8f);

            anim?.SetBool("Jumping", false);
        }
        else if (dir != Vector2.zero)
        {
            // 🟢 Aktifkan animasi jalan dengan sistem Movement milikmu
            float walkInput = (dir.x > 0) ? 1f : (dir.x < 0) ? -1f : 0f;
            float moved = 0f;

            anim?.SetFloat("Walking", 1f); // untuk berjaga kalau Movement belum Update()
            while (moved < moveDistance)
            {
                float step = moveSpeed * Time.deltaTime;
                rb.MovePosition(rb.position + dir * step);
                moved += step;

                // Simulasikan input berjalan agar animasi Movement sinkron
                var moveField = typeof(Movement).GetField("SideMove", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (moveField != null) moveField.SetValue(move, walkInput);

                yield return null;
            }

            // 🔸 Matikan animasi jalan setelah selesai
            anim?.SetFloat("Walking", 0f);
            var sideMoveField = typeof(Movement).GetField("SideMove", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (sideMoveField != null) sideMoveField.SetValue(move, 0f);
        }

        // 🔸 Diam sebentar
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.1f);

        // 🔸 Aktifkan kembali input pemain
        if (input != null)
            input.enabled = true;

        Debug.Log("🎬 Portal exit animation finished (with correct walking animation).");
    }

}