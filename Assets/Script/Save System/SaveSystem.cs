using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    private string savePath;

    [SerializeField] private List<GameObject> prefabDatabase;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "save.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Save(Vector3 playerPos, int mana)
    {
        SaveData data = new SaveData
        {
            playerX = playerPos.x,
            playerY = playerPos.y,
            mana = mana
        };

        GameObject[] placedObjects = GameObject.FindGameObjectsWithTag("Selectable");
        foreach (var obj in placedObjects)
        {
            if (obj == null) continue;

            // ✅ Ambil prefabID tetap
            PrefabID id = obj.GetComponent<PrefabID>();
            string prefabName = (id != null) ? id.PrefabIDValue : obj.name;

            PlacedObjectData pod = new PlacedObjectData
            {
                prefabName = prefabName,
                posX = obj.transform.position.x,
                posY = obj.transform.position.y,
                posZ = obj.transform.position.z,
                rotZ = obj.transform.eulerAngles.z,
                scaleX = obj.transform.localScale.x,
                scaleY = obj.transform.localScale.y,
                scaleZ = obj.transform.localScale.z
            };
            data.placedObjects.Add(pod);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"💾 Game Saved with {data.placedObjects.Count} objects");
    }

        public SaveData Load()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"✅ Game Loaded: {data.placedObjects.Count} objects");
            return data;
        }

        Debug.LogWarning("⚠️ No save file found.");
        return null;
    }

    public SaveData RestoreSave()
    {
        SaveData data = Load();
        if (data == null)
        {
            Debug.LogWarning("⚠️ No save file found.");
            return null;
        }

        // 🧹 Hapus object lama (langsung, tidak menunggu end of frame)
        GameObject[] oldObjects = GameObject.FindGameObjectsWithTag("Selectable");
        foreach (var obj in oldObjects)
        {
            Debug.LogWarning("⚠️ Destroying past object.");
            DestroyImmediate(obj); // ✅ langsung dihapus
        }

        // 🔄 Spawn object dari save
        foreach (var pod in data.placedObjects)
        {
            GameObject prefab = GetPrefabByName(pod.prefabName);
            if (prefab == null) continue;

            GameObject newObj = Instantiate(
                prefab,
                new Vector3(pod.posX, pod.posY, pod.posZ),
                Quaternion.Euler(0, 0, pod.rotZ));
            newObj.transform.localScale = new Vector3(pod.scaleX, pod.scaleY, pod.scaleZ);
        }

        return data; // <— biar bisa dipakai di luar
    }

    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("🗑️ Save data deleted.");
        }
        else
        {
            Debug.LogWarning("⚠️ No save file found to delete.");
        }
    }

    public GameObject GetPrefabByName(string idName)
    {
        foreach (var prefab in prefabDatabase)
        {
            PrefabID id = prefab.GetComponent<PrefabID>();
            if (id != null && id.PrefabIDValue == idName)
                return prefab;
        }
        Debug.LogWarning($"Prefab {idName} not found in database!");
        return null;
    }
        //kalo mau delete pake command di bawha ini, nanti taruh di kode yang mati
        //SaveSystem.Instance.DeleteSave();
}
