using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSpawnManager : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Tunggu 1 frame supaya semua object (termasuk player dan portal) aktif
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 🔹 Pindahkan player ke posisi portal tujuan
            NextSceneTrigger.SetPlayerSpawnPoint(player);
        }

        // Tunggu sedikit sebelum restore object (biar scene stabil)
        yield return new WaitForSeconds(0.2f);

        // 🔹 Load object "Selectable" dari save file khusus scene ini
        if (SaveSystem.Instance != null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            SaveData data = SaveSystem.Instance.LoadSceneData(currentScene);

            if (data != null)
            {
                // Bersihkan object lama dulu
                GameObject[] oldObjects = GameObject.FindGameObjectsWithTag("Selectable");
                foreach (var obj in oldObjects)
                    DestroyImmediate(obj);

                // Spawn ulang object dari save
                foreach (var pod in data.placedObjects)
                {
                    GameObject prefab = SaveSystem.Instance.GetPrefabByName(pod.prefabName);
                    if (prefab == null)
                    {
                        Debug.LogWarning($"⚠️ Prefab '{pod.prefabName}' tidak ditemukan di database!");
                        continue;
                    }

                    GameObject newObj = Instantiate(
                        prefab,
                        new Vector3(pod.posX, pod.posY, pod.posZ),
                        Quaternion.Euler(0, 0, pod.rotZ)
                    );
                    newObj.transform.localScale = new Vector3(pod.scaleX, pod.scaleY, pod.scaleZ);
                }

                Debug.Log($"✅ Restored {data.placedObjects.Count} objects for scene '{currentScene}'");
            }
            else
            {
                Debug.Log($"ℹ️ No saved object data for scene '{SceneManager.GetActiveScene().name}'. Starting fresh.");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ SaveSystem.Instance not found! Cannot restore objects.");
        }
    }
}
