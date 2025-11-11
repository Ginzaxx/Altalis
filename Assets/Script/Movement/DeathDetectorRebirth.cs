using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathDetectorRebirth : MonoBehaviour
{

    public static DeathDetectorRebirth Instance { get; private set; }

    public bool isReloadBecauseDeath = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
