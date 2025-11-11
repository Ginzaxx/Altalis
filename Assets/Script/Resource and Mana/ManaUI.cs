using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaUI : MonoBehaviour
{
    [Header("References")]
    public GameObject manaPrefab;
    public Transform manaContainer;
    private List<GameObject> manaList = new List<GameObject>();

    public void SetMana(int currentMana, int maxMana)
    {
        // Ensure correct number of masks
        while (manaList.Count < maxMana)
        {
            GameObject mask = Instantiate(manaPrefab, manaContainer);
            manaList.Add(mask);
        }

        while (manaList.Count > maxMana)
        {
            Destroy(manaList[manaList.Count - 1]);
            manaList.RemoveAt(manaList.Count - 1);
        }

        // Update mask visuals
        for (int i = 0; i < manaList.Count; i++)
        {
            Image img = manaList[i].GetComponent<Image>();
            img.enabled = i < currentMana;  // show or hide based on health
        }
    }
}