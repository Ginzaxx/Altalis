using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaOrbPieces : MonoBehaviour
{
    public void Awake()
    {
        // call coroutine
        StartCoroutine(PlayYuk());
    }

    IEnumerator PlayYuk()
    {
        // play particle system 
        var externalManaOrb = this.GetComponent<ParticleSystem>().externalForces;
        this.GetComponent<ParticleSystem>().Play();
        yield return new WaitForSeconds(1.0f);
        externalManaOrb.enabled = true;
    }
}
