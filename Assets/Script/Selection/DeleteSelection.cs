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

        foreach (var obj in selectionManager.SelectedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        selectionManager.SelectedObjects.Clear();
        Debug.Log("Deleted selected objects");
    }
}
