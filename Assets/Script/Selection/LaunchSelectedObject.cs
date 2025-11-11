using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LaunchSelectedObjects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public GridSelection gridSelection;
    [SerializeField] private Transform player;

    [Header("Launch Settings")]
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private LayerMask collisionLayers;
    
    [Header("Physics Materials")]
    [SerializeField] private PhysicsMaterial2D frictionMaterial;

    public void StartLaunch(InputAction.CallbackContext context)
    {
        if (gridSelection == null || gridSelection.SelectedObjects.Count == 0) return;

        if (context.performed)
        {
            Debug.Log("Tombol B ditekan! Merging & Launching...");
            MergeAndLaunch();
        }
    }

    void MergeAndLaunch()
    {
        if (player == null)
        {
            Debug.LogWarning("Player belum di-assign!");
            return;
        }

        if (gridSelection.SelectedObjects.Count == 0) return;

        // ✅ STEP 1: Merge objects jadi 1 parent
        GameObject parent = new GameObject("CombinedLaunchObject");
        parent.tag = "Selectable";
        parent.layer = gridSelection.SelectedObjects[0].layer;

        // Cari object paling kiri untuk positioning
        GameObject leftmostObj = null;
        float minX = float.MaxValue;

        List<GameObject> objectsToMerge = new List<GameObject>(gridSelection.SelectedObjects);

        foreach (var obj in objectsToMerge)
        {
            if (obj == null) continue;
            Collider2D col = obj.GetComponent<Collider2D>();
            if (col == null) continue;

            float objMinX = col.bounds.min.x;
            if (objMinX < minX)
            {
                minX = objMinX;
                leftmostObj = obj;
            }
        }

        // Set posisi parent di center object paling kiri
        if (leftmostObj != null)
        {
            Collider2D col = leftmostObj.GetComponent<Collider2D>();
            if (col != null)
                parent.transform.position = col.bounds.center;
            else
                parent.transform.position = leftmostObj.transform.position;
        }

        // Gabung bounds untuk collider besar
        Bounds combinedBounds = new Bounds(objectsToMerge[0].transform.position, Vector3.zero);
        
        // ✅ Simpan posisi parent saat merge untuk hitung offset nanti
        Vector3 parentStartPosition = parent.transform.position;

        // ✅ Simpan data original untuk restore nanti
        List<ChildData> childrenData = new List<ChildData>();

        foreach (var obj in objectsToMerge)
        {
            if (obj == null) continue;

            // Encapsulate bounds dulu sebelum parent
            Collider2D col = obj.GetComponent<Collider2D>();
            if (col != null) combinedBounds.Encapsulate(col.bounds);

            // Simpan data asli
            ChildData data = new ChildData();
            data.originalObject = obj;
            data.worldPosition = obj.transform.position;
            data.worldRotation = obj.transform.rotation;
            data.localScale = obj.transform.localScale;
            
            // Simpan komponen asli
            data.hadRigidbody = obj.GetComponent<Rigidbody2D>() != null;
            data.hadCollider = obj.GetComponent<Collider2D>() != null;
            
            childrenData.Add(data);

            // Parent object
            obj.transform.SetParent(parent.transform);

            // Hapus rigidbody & collider anak
            Rigidbody2D childRB = obj.GetComponent<Rigidbody2D>();
            if (childRB != null) Destroy(childRB);

            Collider2D childCol = obj.GetComponent<Collider2D>();
            if (childCol != null) Destroy(childCol);
        }

        // ✅ Sekarang simpan offset relatif setiap child dari parent
        foreach (var data in childrenData)
        {
            data.offsetFromParent = data.worldPosition - parentStartPosition;
        }

        // ✅ STEP 2: Setup parent physics
        Rigidbody2D rb = parent.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.mass = objectsToMerge.Count;
        rb.gravityScale = 1f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        BoxCollider2D parentCol = parent.AddComponent<BoxCollider2D>();
        parentCol.offset = parent.transform.InverseTransformPoint(combinedBounds.center);
        parentCol.size = combinedBounds.size;

        // Set friction material
        if (frictionMaterial != null)
        {
            parentCol.sharedMaterial = frictionMaterial;
        }

        // ✅ STEP 3: Launch parent object
        Vector2 launchDir = player.localScale.x > 0 ? Vector2.right : Vector2.left;
        rb.velocity = launchDir.normalized * launchForce;
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        // ✅ STEP 4: Add collision handler untuk unmerge nanti
        LaunchCollisionHandler handler = parent.AddComponent<LaunchCollisionHandler>();
        handler.Initialize(this, collisionLayers, childrenData, parentStartPosition);

        Debug.Log($"✅ Merged & launched {objectsToMerge.Count} objects");
    }

    // ✅ Class untuk menyimpan data original child objects
    [System.Serializable]
    private class ChildData
    {
        public GameObject originalObject;
        public Vector3 worldPosition;
        public Quaternion worldRotation;
        public Vector3 localScale;
        public Vector3 offsetFromParent;
        public bool hadRigidbody;
        public bool hadCollider;
    }

    private class LaunchCollisionHandler : MonoBehaviour
    {
        private LayerMask hitLayers;
        private Rigidbody2D rb;
        private LaunchSelectedObjects launcher;
        private bool hasHit = false;
        private List<ChildData> childrenData;
        private Vector3 parentStartPosition;

        public void Initialize(LaunchSelectedObjects src, LayerMask layers, List<ChildData> children, Vector3 startPos)
        {
            launcher = src;
            hitLayers = layers;
            childrenData = children;
            parentStartPosition = startPos;
            rb = GetComponent<Rigidbody2D>();
        }

        // ✅ Platform carrier: Parent player saat collision dari atas
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Handle player parenting first
            if (collision.gameObject.CompareTag("Player"))
            {
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (contact.normal.y < -0.5f) // Player di atas
                    {
                        // Parent player ke platform
                        collision.transform.SetParent(transform);
                        
                        // ✅ Tambahkan component untuk sync velocity
                        PlatformPlayerSync sync = collision.gameObject.GetComponent<PlatformPlayerSync>();
                        if (sync == null)
                        {
                            sync = collision.gameObject.AddComponent<PlatformPlayerSync>();
                        }
                        sync.SetPlatform(rb);
                        
                        Debug.Log("Player on platform - parented with sync");
                        break;
                    }
                }
            }

            // Original collision detection untuk wall hit
            if (hasHit) return;

            // abaikan tabrakan dengan tanah dari bawah
            if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.8f)
                return;

            if (((1 << collision.gameObject.layer) & hitLayers) == 0)
                return;

            hasHit = true;
            Debug.Log($"{name} hit {collision.gameObject.name}, stopping & unmerging...");

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

                // Push away from wall
                Vector2 pushDir = collision.contacts[0].normal;
                transform.position += (Vector3)(pushDir * 0.05f);

                // Add downward force
                rb.AddForce(Vector2.down * 2f, ForceMode2D.Impulse);
            }

            StartCoroutine(UnmergeAndRestore());
        }

        // ✅ Unparent player saat keluar dari platform
        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (collision.transform.parent == transform)
                {
                    collision.transform.SetParent(null);
                    
                    // Remove sync component
                    PlatformPlayerSync sync = collision.gameObject.GetComponent<PlatformPlayerSync>();
                    if (sync != null)
                    {
                        Destroy(sync);
                    }
                    
                    Debug.Log("Player left platform - unparented");
                }
            }
        }

        private IEnumerator UnmergeAndRestore()
        {
            yield return new WaitForSeconds(0.3f);

            // ✅ Unparent player jika masih menempel sebelum destroy parent
            Transform[] allChildren = GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if (child.CompareTag("Player") && child.parent == transform)
                {
                    child.SetParent(null);
                    
                    // Remove sync component
                    PlatformPlayerSync sync = child.GetComponent<PlatformPlayerSync>();
                    if (sync != null)
                    {
                        Destroy(sync);
                    }
                    
                    Debug.Log("Player unparented before unmerge");
                }
            }

            // ✅ UNMERGE: Restore semua child objects
            GameObject parentObj = gameObject;
            Vector3 parentCurrentPos = parentObj.transform.position;

            Debug.Log($"Parent start: {parentStartPosition}, Parent now: {parentCurrentPos}");

            foreach (var data in childrenData)
            {
                if (data.originalObject == null) continue;

                // Unparent
                data.originalObject.transform.SetParent(null);

                // ✅ Gunakan offset yang sudah dihitung
                Vector3 newPosition = parentCurrentPos + data.offsetFromParent;
                
                data.originalObject.transform.position = newPosition;
                data.originalObject.transform.rotation = data.worldRotation;
                data.originalObject.transform.localScale = data.localScale;

                // ✅ Restore Rigidbody & Collider
                Rigidbody2D childRb = data.originalObject.GetComponent<Rigidbody2D>();
                if (childRb == null && data.hadRigidbody)
                {
                    childRb = data.originalObject.AddComponent<Rigidbody2D>();
                }
                else if (childRb == null)
                {
                    childRb = data.originalObject.AddComponent<Rigidbody2D>();
                }

                if (childRb != null)
                {
                    childRb.isKinematic = false;
                    childRb.gravityScale = 1f;
                    childRb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                    childRb.interpolation = RigidbodyInterpolation2D.Interpolate;
                    
                    // Beri sedikit push ke bawah agar jatuh
                    childRb.velocity = Vector2.down * 2f;
                }

                BoxCollider2D childCol = data.originalObject.GetComponent<BoxCollider2D>();
                if (childCol == null && data.hadCollider)
                {
                    childCol = data.originalObject.AddComponent<BoxCollider2D>();
                }
                else if (childCol == null)
                {
                    childCol = data.originalObject.AddComponent<BoxCollider2D>();
                }

                // Set friction material
                if (childCol != null && launcher.frictionMaterial != null)
                {
                    childCol.sharedMaterial = launcher.frictionMaterial;
                }

                Debug.Log($"✅ Restored: {data.originalObject.name} at {newPosition} (offset: {data.offsetFromParent})");
            }

            // ✅ Destroy parent object
            Destroy(parentObj);
            Debug.Log("✅ Unmerge complete!");

            Destroy(this);
        }
    }

    // ✅ Component untuk sync velocity player dengan platform
    private class PlatformPlayerSync : MonoBehaviour
    {
        private Rigidbody2D platformRb;
        private Rigidbody2D playerRb;

        private void Start()
        {
            playerRb = GetComponent<Rigidbody2D>();
        }

        public void SetPlatform(Rigidbody2D platform)
        {
            platformRb = platform;
            if (playerRb == null) playerRb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (platformRb != null && playerRb != null)
            {
                // ✅ Tambahkan velocity platform ke player setiap frame
                Vector2 platformVelocity = platformRb.velocity;
                Vector2 currentVelocity = playerRb.velocity;
                
                // Override X velocity dengan platform velocity
                playerRb.velocity = new Vector2(platformVelocity.x, currentVelocity.y);
            }
        }
    }
}