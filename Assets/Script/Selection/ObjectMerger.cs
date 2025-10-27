using UnityEngine;
using System.Collections.Generic;

public class PhysicsCombiner : MonoBehaviour
{
    [SerializeField] private CopyPlacement duplicatePlacement;

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

        GameObject parent = new GameObject("CombinedObject");
        parent.tag = "Selectable";

        Rigidbody2D rb = parent.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;

        // 🔥 Cari object paling kiri berdasarkan bounds.min.x
        GameObject leftmostObj = null;
        float minX = float.MaxValue;

        foreach (var obj in objects)
        {
            if (obj == null) continue;
            Collider2D col = obj.GetComponent<Collider2D>();
            if (col == null) continue;

            float objMinX = col.bounds.min.x; // sisi kiri object
            if (objMinX < minX)
            {
                minX = objMinX;
                leftmostObj = obj;
            }
        }

        // 🔥 Gunakan center dari bounds object paling kiri
        if (leftmostObj != null)
        {
            Collider2D col = leftmostObj.GetComponent<Collider2D>();
            if (col != null)
                parent.transform.position = col.bounds.center;
            else
                parent.transform.position = leftmostObj.transform.position;
        }

        // Gabung bounds untuk collider besar
        Bounds combinedBounds = new Bounds(objects[0].transform.position, Vector3.zero);
        foreach (var obj in objects)
        {
            if (obj == null) continue;
            Collider2D col = obj.GetComponent<Collider2D>();
            if (col != null) combinedBounds.Encapsulate(col.bounds);

            obj.transform.SetParent(parent.transform);

            Rigidbody2D childRB = obj.GetComponent<Rigidbody2D>();
            if (childRB != null) Destroy(childRB);

            Collider2D childCol = obj.GetComponent<Collider2D>();
            if (childCol != null) Destroy(childCol); // 🔥 buang collider anak
        }

        // Tambahkan BoxCollider2D di parent sesuai bounds gabungan
        BoxCollider2D parentCol = parent.AddComponent<BoxCollider2D>();
        parentCol.offset = parent.transform.InverseTransformPoint(combinedBounds.center);
        parentCol.size = combinedBounds.size;

        Debug.Log($"✅ Combined {objects.Count} objects. Parent at center of leftmost object.");
    }
}
