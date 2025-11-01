using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Menghapus beberapa object dari scene lewat Timeline Signal,
/// dengan efek VFX opsional di setiap object yang dihancurkan.
/// </summary>
public class TimelineMultiObjectDestroyer : MonoBehaviour
{
    [Header("Daftar Object yang Akan Dihapus")]
    public GameObject[] targetObjects;

    [Header("Efek Visual (Opsional)")]
    [Tooltip("Prefab efek yang akan dipanggil di posisi object sebelum dihancurkan.")]
    public GameObject destroyVfxPrefab;
    [Tooltip("Offset posisi efek dari posisi object.")]
    public Vector3 vfxOffset = Vector3.zero;

    [Header("Pengaturan Tambahan")]
    public bool destroyChildren = false;
    public bool logAction = true;

    /// <summary>
    /// Dipanggil dari Timeline Signal untuk menghapus semua object di daftar.
    /// </summary>
    public void DestroyObjectsFromTimeline()
    {
        if (targetObjects == null || targetObjects.Length == 0)
        {
            Debug.LogWarning("⚠️ [TimelineMultiObjectDestroyer] Tidak ada object yang diisi di daftar!");
            return;
        }

        foreach (var obj in targetObjects)
        {
            if (obj == null) continue;

            // 🔥 Tampilkan efek sebelum dihapus
            if (destroyVfxPrefab != null)
            {
                Instantiate(destroyVfxPrefab, obj.transform.position + vfxOffset, Quaternion.identity);
            }

            // 🧩 Hapus anak-anak jika diaktifkan
            if (destroyChildren)
            {
                foreach (Transform child in obj.transform)
                {
                    if (child != null)
                        Destroy(child.gameObject);
                }
            }

            if (logAction)
                Debug.Log($"🗑️ [TimelineMultiObjectDestroyer] Destroyed {obj.name}");

            // 💀 Hapus object
            Destroy(obj);
        }
    }

    /// <summary>
    /// (Opsional) Ganti daftar target secara runtime.
    /// </summary>
    public void SetTargets(GameObject[] newTargets)
    {
        targetObjects = newTargets;

        if (logAction)
            Debug.Log($"🎯 [TimelineMultiObjectDestroyer] Target diganti. Jumlah baru: {newTargets?.Length ?? 0}");
    }
}
