using UnityEngine;

public class DeleteSelection : MonoBehaviour
{
    [SerializeField] private GridSelection gridSelection;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            DeleteSelectedObjects();
        }
    }

    void DeleteSelectedObjects()
    {
        if (gridSelection.SelectedObjects.Count == 0)
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

        foreach (var obj in gridSelection.SelectedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        gridSelection.SelectedObjects.Clear();
        Debug.Log("Deleted selected objects (cost 1 mana)");
    }
}
