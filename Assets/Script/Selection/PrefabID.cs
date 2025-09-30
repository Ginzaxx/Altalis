using UnityEngine;

public class PrefabID : MonoBehaviour
{
    [SerializeField] private string prefabID; // ID tetap untuk mengenali prefab asli

    public string PrefabIDValue => prefabID;

    private void Reset()
    {
        // otomatis isi dengan nama prefab (sekali aja waktu bikin prefab)
        prefabID = gameObject.name;
    }
}
