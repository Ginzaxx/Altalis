using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrumbleTile : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // start the timer
            StartCoroutine(startCrumbleTiles());
        }
    }


    IEnumerator startCrumbleTiles()
    {
        yield return new WaitForSeconds(1.2f);
        Destroy(this.gameObject);
    }
}
