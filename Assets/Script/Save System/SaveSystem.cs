using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    private string savePath;
    [SerializeField] private List<GameObject> prefabDatabase;

    // 🔹 Simpan mana blocks yang sudah pernah diaktifkan
    private HashSet<string> triggeredManaBlocks = new HashSet<string>();

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

    private string GetSceneSavePath(string sceneName)
    {
        return Path.Combine(Application.persistentDataPath, $"save_{sceneName}.json");
    }

    public bool IsManaBlockTriggered(string id) => triggeredManaBlocks.Contains(id);

    public void SetManaBlockTriggered(string id)
    {
        if (!triggeredManaBlocks.Contains(id))
            triggeredManaBlocks.Add(id);
    }

    // ============================================================
    // 🟢 SAVE
    // ============================================================
    public void Save(Vector3 playerPos, int mana)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        SaveData data = new SaveData
        {
            sceneName = sceneName,
            playerX = playerPos.x,
            playerY = playerPos.y,
            mana = mana,
            triggeredManaBlocks = new List<string>(triggeredManaBlocks),

            // 🔹 tambahan untuk resource & gold
            maxMana = ResourceManager.Instance != null ? ResourceManager.Instance.MaxMana : 0,
            selectionLimit = ResourceManager.Instance != null ? ResourceManager.Instance.SelectLimit : 0,
            gold = GoldManager.Instance != null ? GoldManager.Instance.gold : 0
        };

        // Simpan semua selectable object
        GameObject[] placedObjects = GameObject.FindGameObjectsWithTag("Selectable");
        foreach (var obj in placedObjects)
        {
            if (obj == null) continue;
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

        // Simpan ke file
        string scenePath = GetSceneSavePath(sceneName);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        File.WriteAllText(scenePath, json);

        Debug.Log($"💾 Saved scene '{sceneName}' | Mana: {data.mana}, MaxMana: {data.maxMana}, SelectLimit: {data.selectionLimit}, Gold: {data.gold}");
    }

    // ============================================================
    // 🟡 LOAD
    // ============================================================
    public SaveData Load()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data.triggeredManaBlocks != null)
                triggeredManaBlocks = new HashSet<string>(data.triggeredManaBlocks);

            // 🔹 Apply langsung ke ResourceManager dan GoldManager
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.LoadStatsFromSave(data.maxMana, data.selectionLimit);
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.LoadGoldFromSave(data.gold);
            }

            Debug.Log($"✅ Loaded global save ({data.placedObjects.Count} objects, {triggeredManaBlocks.Count} ManaBlocks triggered)");
            return data;
        }

        Debug.LogWarning("⚠️ No global save file found.");
        return null;
    }

    // ============================================================
    // 🟣 LOAD PER SCENE
    // ============================================================
    public SaveData LoadSceneData(string sceneName)
    {
        string scenePath = GetSceneSavePath(sceneName);
        if (File.Exists(scenePath))
        {
            string json = File.ReadAllText(scenePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data.triggeredManaBlocks != null)
                triggeredManaBlocks = new HashSet<string>(data.triggeredManaBlocks);

            Debug.Log($"✅ Loaded scene save '{sceneName}' ({data.placedObjects.Count} objects)");
            return data;
        }

        Debug.LogWarning($"⚠️ No save found for scene '{sceneName}'.");
        return null;
    }

    // ============================================================
    // 🔵 RESTORE SAVE
    // ============================================================
    public SaveData RestoreSave()
    {
        SaveData data = Load();
        if (data == null) return null;

        string currentScene = SceneManager.GetActiveScene().name;
        if (data.sceneName != currentScene)
        {
            Debug.Log($"⚠️ Save ditemukan untuk scene '{data.sceneName}', bukan '{currentScene}'. Abaikan restore.");
            return null;
        }

        // Hapus object lama
        GameObject[] oldObjects = GameObject.FindGameObjectsWithTag("Selectable");
        foreach (var obj in oldObjects)
            DestroyImmediate(obj);

        // Spawn ulang
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

        // 🔹 Pulihkan stats resource dan gold
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.LoadStatsFromSave(data.maxMana, data.selectionLimit);
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.LoadGoldFromSave(data.gold);
        }

        return data;
    }

    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("🗑️ Deleted global save.json");
        }
    }

    public void LoadLastScene()
    {
        SaveData data = Load();
        if (data == null)
        {
            Debug.LogWarning("⚠️ No global save found, cannot load scene.");
            return;
        }

        if (!string.IsNullOrEmpty(data.sceneName))
        {
            Debug.Log($"🔄 Loading last played scene: {data.sceneName}");
            SceneManager.LoadScene(data.sceneName);
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
            Debug.Log($"🗑️ Deleted: {Path.GetFileName(file)}");
        }

        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("🗑️ Deleted main save.json");
        }

        triggeredManaBlocks.Clear();
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

    // ============================================================
    // 🟢 SAVE SPECIAL (ManaBlock)
    // ============================================================
    public void SaveSpecial(string id, Vector3 playerPos, int mana)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string specialPath = Path.Combine(Application.persistentDataPath, $"save_manaBlock_{id}.json");
        string scenePath = GetSceneSavePath(sceneName);

        SaveData data = new SaveData
        {
            sceneName = sceneName,
            playerX = playerPos.x,
            playerY = playerPos.y,
            mana = mana,
            triggeredManaBlocks = new List<string>(triggeredManaBlocks),
            maxMana = ResourceManager.Instance != null ? ResourceManager.Instance.MaxMana : 0,
            selectionLimit = ResourceManager.Instance != null ? ResourceManager.Instance.SelectLimit : 0,
            gold = GoldManager.Instance != null ? GoldManager.Instance.gold : 0
        };

        GameObject[] placedObjects = GameObject.FindGameObjectsWithTag("Selectable");
        foreach (var obj in placedObjects)
        {
            if (obj == null) continue;
            PrefabID idComp = obj.GetComponent<PrefabID>();
            string prefabName = idComp ? idComp.PrefabIDValue : obj.name;

            data.placedObjects.Add(new PlacedObjectData
            {
                prefabName = prefabName,
                posX = obj.transform.position.x,
                posY = obj.transform.position.y,
                posZ = obj.transform.position.z,
                rotZ = obj.transform.eulerAngles.z,
                scaleX = obj.transform.localScale.x,
                scaleY = obj.transform.localScale.y,
                scaleZ = obj.transform.localScale.z
            });
        }

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(specialPath, json);
        File.WriteAllText(savePath, json);
        File.WriteAllText(scenePath, json);

        Debug.Log($"💾 Saved ManaBlock '{id}' | MaxMana: {data.maxMana}, Gold: {data.gold}, SelectLimit: {data.selectionLimit}");
    }

    // ============================================================
    // 🔵 RESTORE SPECIAL
    // ============================================================
    public void RestoreSpecial(string id)
    {
        string path = Path.Combine(Application.persistentDataPath, $"save_manaBlock_{id}.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"⚠️ No save found for ManaBlock '{id}'!");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data.triggeredManaBlocks != null)
            triggeredManaBlocks = new HashSet<string>(data.triggeredManaBlocks);

        if (data.sceneName != SceneManager.GetActiveScene().name)
        {
            Debug.LogWarning($"⚠️ ManaBlock '{id}' save belongs to '{data.sceneName}', not current scene.");
            return;
        }

        GameObject[] oldObjects = GameObject.FindGameObjectsWithTag("Selectable");
        foreach (var obj in oldObjects)
            Object.DestroyImmediate(obj);

        foreach (var pod in data.placedObjects)
        {
            GameObject prefab = GetPrefabByName(pod.prefabName);
            if (prefab == null) continue;

            GameObject newObj = Object.Instantiate(prefab,
                new Vector3(pod.posX, pod.posY, pod.posZ),
                Quaternion.Euler(0, 0, pod.rotZ));

            newObj.transform.localScale = new Vector3(pod.scaleX, pod.scaleY, pod.scaleZ);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.transform.position = new Vector3(data.playerX, data.playerY, player.transform.position.z);

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.LoadStatsFromSave(data.maxMana, data.selectionLimit);
            ResourceManager.Instance.FullRestoreMana();
        }

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.LoadGoldFromSave(data.gold);
        }

        Debug.Log($"✅ Restored ManaBlock '{id}' | MaxMana {data.maxMana}, SelectLimit {data.selectionLimit}, Gold {data.gold}");
    }
}
