using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    private string savePath;
    [SerializeField] private List<GameObject> prefabDatabase;

    private string lastManaBlockID;
    public void SetLastManaBlock(string id)
    {
        lastManaBlockID = id;
    }
    public string GetLastManaBlockID() => lastManaBlockID;

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

    private GameObject[] FindGameObjectsWithTags(params string[] tags)
    {
        List<GameObject> results = new List<GameObject>();
        foreach (var tag in tags)
        {
            results.AddRange(GameObject.FindGameObjectsWithTag(tag));
        }
        return results.ToArray();
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
        GameObject[] placedObjects = FindGameObjectsWithTags("Selectable", "ManaOrb");
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
        GameObject[] oldObjects = FindGameObjectsWithTags("Selectable", "ManaOrb");
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
        // 🟢 Simpan ID blok terakhir yang diaktifkan
        lastManaBlockID = id;

        string sceneName = SceneManager.GetActiveScene().name;
        string specialPath = Path.Combine(Application.persistentDataPath, $"save_manaBlock_{id}.json");
        string scenePath = GetSceneSavePath(sceneName);

        // 🟣 Kumpulkan semua data penting
        SaveData data = new SaveData
        {
            sceneName = sceneName,
            playerX = playerPos.x,
            playerY = playerPos.y,
            mana = mana,
            triggeredManaBlocks = new List<string>(triggeredManaBlocks),

            lastManaBlockID = lastManaBlockID
        };

        // 🧱 Simpan semua objek yang ada di scene
        GameObject[] placedObjects = FindGameObjectsWithTags("Selectable", "ManaOrb");
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
        // 💾 Simpan ke tiga file (khusus blok, global, dan per scene)
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(specialPath, json);
        File.WriteAllText(savePath, json);
        File.WriteAllText(scenePath, json);

        Debug.Log($"💾 Saved ManaBlock '{id}' | Mana: {data.mana}");
    }

    // ============================================================
    // 🔵 RESTORE mana block
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

        // 🔹 Pulihkan daftar mana block yang sudah terpicu
        if (data.triggeredManaBlocks != null)
            triggeredManaBlocks = new HashSet<string>(data.triggeredManaBlocks);

        // 🔹 Cegah restore dari scene berbeda
        string currentScene = SceneManager.GetActiveScene().name;
        if (data.sceneName != currentScene)
        {
            Debug.LogWarning($"⚠️ ManaBlock '{id}' save belongs to '{data.sceneName}', not current scene '{currentScene}'.");
            return;
        }

        // 🔹 Hapus semua objek lama (Selectable & ManaOrb)
        GameObject[] oldObjects = FindGameObjectsWithTags("Selectable", "ManaOrb");
        foreach (var obj in oldObjects)
        {
            if (obj != null)
                Object.Destroy(obj);
        }

        // 🔹 Spawn ulang semua object dari data save
        foreach (var pod in data.placedObjects)
        {
            GameObject prefab = GetPrefabByName(pod.prefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"⚠️ Prefab '{pod.prefabName}' not found in database!");
                continue;
            }

            GameObject newObj = Object.Instantiate(prefab,
                new Vector3(pod.posX, pod.posY, pod.posZ),
                Quaternion.Euler(0, 0, pod.rotZ));

            newObj.transform.localScale = new Vector3(pod.scaleX, pod.scaleY, pod.scaleZ);
        }

        // 🔹 Pindahkan posisi player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.transform.position = new Vector3(data.playerX, data.playerY, player.transform.position.z);

        // 🔹 Simpan kembali ID blok terakhir agar UndoManager tahu blok mana yang terakhir
        lastManaBlockID = data.lastManaBlockID;

        Debug.Log($"✅ Restored ManaBlock '{id}' | Mana: {data.mana}, MaxMana: {data.maxMana}, Gold: {data.gold}, SelectLimit: {data.selectionLimit}");
    }

        //Save saat ganti scene
        public void SavePlacedObjectsOnly()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        SaveData data = new SaveData
        {
            sceneName = sceneName,
            placedObjects = new List<PlacedObjectData>()
        };

        // 🔹 Ambil semua object bertag Selectable dan ManaOrb
        GameObject[] placedObjects = FindGameObjectsWithTags("Selectable", "ManaOrb");

        foreach (var obj in placedObjects)
        {
            if (obj == null) continue;
            PrefabID id = obj.GetComponent<PrefabID>();
            string prefabName = id != null ? id.PrefabIDValue : obj.name;

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

        // 🔹 Simpan ke file save khusus per scene
        string scenePath = GetSceneSavePath(sceneName);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(scenePath, json);

        Debug.Log($"💾 [SavePlacedObjectsOnly] Saved {data.placedObjects.Count} objects in scene '{sceneName}'.");
    }

    public void LoadPlacedObjectsOnly()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string scenePath = GetSceneSavePath(sceneName);

        if (!File.Exists(scenePath))
        {
            Debug.LogWarning($"⚠️ No saved object data found for scene '{sceneName}'.");
            return;
        }

        string json = File.ReadAllText(scenePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // 🔹 Hapus object lama sebelum spawn ulang
        GameObject[] oldObjects = FindGameObjectsWithTags("Selectable", "ManaOrb");
        foreach (var obj in oldObjects)
            DestroyImmediate(obj);

        // 🔹 Spawn ulang object yang disimpan
        foreach (var pod in data.placedObjects)
        {
            GameObject prefab = GetPrefabByName(pod.prefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab '{pod.prefabName}' not found in database!");
                continue;
            }

            GameObject newObj = Instantiate(
                prefab,
                new Vector3(pod.posX, pod.posY, pod.posZ),
                Quaternion.Euler(0, 0, pod.rotZ)
            );

            newObj.transform.localScale = new Vector3(pod.scaleX, pod.scaleY, pod.scaleZ);
        }

        Debug.Log($"✅ [LoadPlacedObjectsOnly] Loaded {data.placedObjects.Count} objects in scene '{sceneName}'.");
    }

    public void RestoreLastManaBlock()
    {
        if (string.IsNullOrEmpty(lastManaBlockID))
        {
            Debug.LogWarning("⚠️ No last ManaBlock ID recorded. Cannot restore!");
            return;
        }

        Debug.Log($"🔄 Restoring last ManaBlock save: {lastManaBlockID}");
        RestoreSpecial(lastManaBlockID);
    }
}
