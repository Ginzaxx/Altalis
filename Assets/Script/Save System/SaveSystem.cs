using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

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

            // Auto-load prefabs dari Resources/Prefabs kalau inspector kosong
            if (prefabDatabase == null || prefabDatabase.Count == 0)
            {
                prefabDatabase = Resources.LoadAll<GameObject>("Prefabs").ToList();
                Debug.Log($"📦 Loaded {prefabDatabase.Count} prefabs from Resources/Prefabs");
            }
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
            sceneName = SceneManager.GetActiveScene().name,
            playerX = playerPos.x,
            playerY = playerPos.y,
            mana = mana
        };

        // 🔹 Gabungkan semua object yang ingin disave (Selectable + ManaOrb)
        List<GameObject> objectsToSave = new List<GameObject>();
        objectsToSave.AddRange(GameObject.FindGameObjectsWithTag("Selectable"));
        objectsToSave.AddRange(GameObject.FindGameObjectsWithTag("ManaOrb"));

        foreach (var obj in objectsToSave)
        {
            if (obj == null) continue;

            PrefabID id = obj.GetComponent<PrefabID>();
            string prefabName = (id != null && !string.IsNullOrEmpty(id.PrefabIDValue))
                                ? id.PrefabIDValue
                                : obj.name.Replace("(Clone)", "").Trim();

            PlacedObjectData pod = new PlacedObjectData
            {
                prefabName = prefabName,
                tag = obj.tag,
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
        Debug.Log($"💾 Game Saved in scene {data.sceneName} with {data.placedObjects.Count} objects");
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
        if (data == null) return null;

        string currentScene = SceneManager.GetActiveScene().name;
        if (data.sceneName != currentScene)
        {
            Debug.Log($"⚠️ Save ditemukan, tapi untuk scene '{data.sceneName}', bukan '{currentScene}'. Abaikan restore.");
            return null;
        }

        // 🔹 Hapus object lama (Selectable + ManaOrb)
        List<GameObject> oldObjects = new List<GameObject>();
        oldObjects.AddRange(GameObject.FindGameObjectsWithTag("Selectable"));
        oldObjects.AddRange(GameObject.FindGameObjectsWithTag("ManaOrb"));

        foreach (var obj in oldObjects)
        {
            DestroyImmediate(obj);
        }

        // 🔹 Spawn object dari save
        foreach (var pod in data.placedObjects)
        {
            GameObject prefab = GetPrefabByName(pod.prefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"❌ Prefab {pod.prefabName} not found. Skip spawn.");
                continue;
            }

            GameObject newObj = Instantiate(
                prefab,
                new Vector3(pod.posX, pod.posY, pod.posZ),
                Quaternion.Euler(0, 0, pod.rotZ));
            newObj.transform.localScale = new Vector3(pod.scaleX, pod.scaleY, pod.scaleZ);

            // Restore tag kalau ada
            if (!string.IsNullOrEmpty(pod.tag))
            {
                try { newObj.tag = pod.tag; }
                catch { Debug.LogWarning($"⚠️ Tag {pod.tag} belum ada di Tag Manager."); }
            }
        }
        return data;
    }

    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("🗑️ Save data deleted.");
        }
    }

    public GameObject GetPrefabByName(string idName)
    {
        string normalized = idName.Replace("(Clone)", "").Trim();

        foreach (var prefab in prefabDatabase)
        {
            if (prefab == null) continue;

            PrefabID id = prefab.GetComponent<PrefabID>();
            if (id != null && id.PrefabIDValue == normalized)
                return prefab;

            if (prefab.name == normalized)
                return prefab;
        }
        Debug.LogWarning($"Prefab {idName} not found in database!");
        return null;
    }

    public void LoadLastScene()
    {
        SaveData data = Load();
        if (data == null)
        {
            Debug.LogWarning("⚠️ No save found, cannot load scene.");
            return;
        }

        string sceneToLoad = data.sceneName;
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"🔄 Loading last played scene: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("⚠️ Save file has no scene info.");
        }
    }

    public void DeleteAllSaves()
    {
        string dir = Application.persistentDataPath;
        string[] files = Directory.GetFiles(dir, "save_*.json");

        foreach (var file in files)
        {
            File.Delete(file);
            Debug.Log($"🗑️ Deleted save file: {Path.GetFileName(file)}");
        }

        string mainSave = Path.Combine(dir, "save.json");
        if (File.Exists(mainSave))
        {
            File.Delete(mainSave);
            Debug.Log("🗑️ Deleted main save.json");
        }
    }
}
