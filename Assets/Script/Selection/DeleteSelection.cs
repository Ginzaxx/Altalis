using UnityEngine;

public class DeleteSelection : MonoBehaviour
{
    [SerializeField] private DragSelection selectionManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            DeleteSelectedObjects();
        }
    }

    void DeleteSelectedObjects()
    {
        if (selectionManager.SelectedObjects.Count == 0)
        {
            Debug.Log("No objects selected to delete.");
            return;
        }

        // 🔥 Cek mana dulu
        if (ResourceManager.Instance == null || ResourceManager.Instance.CurrentMana < 1)
        {
            Debug.Log("❌ Not enough mana to delete objects!");
            return;
        }

        // 🔥 Kurangi 1 mana
        ResourceManager.Instance.SpendMana(1);

        foreach (var obj in selectionManager.SelectedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        selectionManager.SelectedObjects.Clear();
        Debug.Log("Deleted selected objects (cost 1 mana)");
    }
}
