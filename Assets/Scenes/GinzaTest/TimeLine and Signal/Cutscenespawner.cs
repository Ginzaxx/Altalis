using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Tilemaps; // ✅ Tambahan penting untuk Tilemap

/// <summary>
/// Memungkinkan Timeline memunculkan (place) objek prefab langsung di scene.
/// Tidak tergantung pada sistem selection CopyPlacement.
/// Cukup dipanggil dari Timeline Signal (Signal Receiver → PlacePrefabFromTimeline).
/// </summary>
public class TimelineObjectPlacer : MonoBehaviour
{
    [Header("Prefab & Target Tilemap")]
    public GameObject prefabToPlace;
    public Tilemap targetTilemap;

    [Header("Spawn Settings")]
    public bool useGridSnap = true;
    public Vector3 spawnPosition;      // posisi manual
    public Transform spawnTransform;   // posisi dinamis dari scene

    [Header("VFX & Events")]
    public GameObject placeVfxPrefab;
    public static event System.Action<GameObject> OnObjectPlaced;

    [Header("Debug")]
    public bool logPlacement = true;

    /// <summary>
    /// Dipanggil dari Timeline Signal untuk menempatkan prefab di dunia game.
    /// </summary>
    public void PlacePrefabFromTimeline()
    {
        if (prefabToPlace == null)
        {
            Debug.LogWarning("⚠️ [TimelineObjectPlacer] No prefab assigned!");
            return;
        }

        // Tentukan posisi spawn
        Vector3 finalPosition = spawnTransform != null
            ? spawnTransform.position
            : spawnPosition;

        // Snap ke grid jika diaktifkan
        if (useGridSnap && targetTilemap != null)
        {
            Vector3Int cell = targetTilemap.WorldToCell(finalPosition);
            finalPosition = targetTilemap.GetCellCenterWorld(cell);
        }

        // Spawn prefab
        GameObject obj = Instantiate(prefabToPlace, finalPosition, Quaternion.identity);

        // Tambah efek visual jika ada
        if (placeVfxPrefab != null)
            Instantiate(placeVfxPrefab, finalPosition, Quaternion.identity);

        // Tambahkan tag default kalau belum ada
        if (string.IsNullOrEmpty(obj.tag) || obj.tag == "Untagged")
            obj.tag = "Selectable";

        // Invoke event (bisa di-listen sistem lain)
        OnObjectPlaced?.Invoke(obj);

        if (logPlacement)
            Debug.Log($"🎬 [TimelineObjectPlacer] Placed {obj.name} at {finalPosition}");
    }

    /// <summary>
    /// (Opsional) Bisa dipanggil lewat signal juga untuk mempersiapkan data spawn dinamis.
    /// </summary>
    public void SetSpawnPrefab(GameObject newPrefab)
    {
        prefabToPlace = newPrefab;
    }

    /// <summary>
    /// (Opsional) Ubah posisi spawn lewat script atau Timeline signal.
    /// </summary>
    public void SetSpawnPosition(Vector3 position)
    {
        spawnPosition = position;
    }
}
