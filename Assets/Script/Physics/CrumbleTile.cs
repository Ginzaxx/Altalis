using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrumbleTile : MonoBehaviour
{

    [SerializeField] private float shakeMagnitude = 0.1f; // How intense the shake is
    [SerializeField] private Transform spriteTransform; // Reference to the sprite's Transform (child GameObject)

    [SerializeField] private ParticleSystem dustCrumbleTiles;
    void Start()
    {
        // If spriteTransform is not assigned, try to find a child with SpriteRenderer
        if (spriteTransform == null)
        {
            spriteTransform = GetComponentInChildren<SpriteRenderer>()?.transform;
            if (spriteTransform == null)
            {
                Debug.LogError("CrumbleTile: No spriteTransform assigned or found in children!", this);
            }
        }

        // Store the sprite's original local position
        spriteOriginalLocalPosition = spriteTransform != null ? spriteTransform.localPosition : Vector3.zero;
    }
    private Vector3 spriteOriginalLocalPosition; // Store the sprite's original local position
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // start the timer
            StartCoroutine(startCrumbleTiles());
        }
    }


    // IEnumerator startCrumbleTiles()
    // {
    //     yield return new WaitForSeconds(1.2f);
    //     Destroy(this.gameObject);
    // }

    IEnumerator startCrumbleTiles()
    {
        // Shake for 1.0f seconds, then wait 0.2f more before destroying
        float shakeDuration = 1.0f;
        float remainingShakeTime = shakeDuration;

        while (remainingShakeTime > 0 && spriteTransform != null)
        {
            //play particle system dust
            dustCrumbleTiles.Play();
            // Randomly offset sprite's local position for shaking
            Vector2 shakeOffset = Random.insideUnitCircle * shakeMagnitude;
            spriteTransform.localPosition = spriteOriginalLocalPosition + new Vector3(shakeOffset.x, 0, 0);

            remainingShakeTime -= Time.deltaTime;
            yield return null; // Wait one frame
        }

        // Reset sprite position after shaking
        spriteTransform.localPosition = spriteOriginalLocalPosition;

        // Wait the remaining time (0.2f) before crumbling
        yield return new WaitForSeconds(0.2f);
        Destroy(this.gameObject);
    }
}
