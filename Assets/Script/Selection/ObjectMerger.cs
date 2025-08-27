using UnityEngine;
using System.Collections.Generic;

public class PhysicsCombiner : MonoBehaviour
{
    [SerializeField] private DuplicatePlacement duplicatePlacement;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (duplicatePlacement != null && duplicatePlacement.GetLastPlacedObjects().Count > 0)
            {
                CombineObjects(duplicatePlacement.GetLastPlacedObjects());
            }
        }
    }

    void CombineObjects(List<GameObject> objects)
    {
        if (objects == null || objects.Count == 0) return;

        // Buat parent kosong
        GameObject parent = new GameObject("CombinedObject");
        parent.tag = "Selectable";

        // Tambahkan Rigidbody2D utama
        Rigidbody2D rb = parent.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Cari posisi tengah dari semua object
        Vector3 center = Vector3.zero;
        foreach (var obj in objects)
        {
            center += obj.transform.position;
        }
        center /= objects.Count;
        parent.transform.position = center;

        // Pindahkan semua object jadi child
        foreach (var obj in objects)
        {
            if (obj == null) continue;

            obj.transform.SetParent(parent.transform);

            // Buang rigidbody lama (supaya tidak bentrok)
            Rigidbody2D childRB = obj.GetComponent<Rigidbody2D>();
            if (childRB != null) Destroy(childRB);
        }

        Debug.Log($"✅ Combined {objects.Count} objects into one physics body.");
    }
}
