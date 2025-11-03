using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public int goldValue = 1;
    private Transform playerPosition;

    // CONFIGURATION
    [Header("Target & Timing")]
    [Tooltip("How long it takes for the coin to reach the player.")]
    public float attractionDuration = 0.5f;

    [Tooltip("The 'ease mode' for the attraction movement.")]
    public EaseType easeType = EaseType.EaseIn;

    [Header("Particle Effect Splash")]
    public GameObject SplashEffect;

    // Enum to define the "ease mode" in the inspector
    public enum EaseType { Linear, EaseIn, EaseOut, EaseInOut }

    //start
    void Start()
    {
        playerPosition = GameObject.Find("Player").transform;
        // start as soon as this coin get instantiated (called ienumerator).
        if (playerPosition != null)
        {
            StartCoroutine(WaitAndGetMagnetized());
        }
        else
        {
            Debug.Log("GameObjek dengan nama 'Player' ga ditemuin goblok!");
        }


    }

    IEnumerator WaitAndGetMagnetized()
    {
        yield return new WaitForSeconds(2f);
        // after wait. get the player position and then follow it.

        float attractionTimer = 0;
        Vector3 CoinPosition = this.transform.position;

        while (attractionTimer <= attractionDuration)
        {

            // Safety check: What if the player is destroyed while
            // the coin is moving? This 'yield break' will
            // safely exit the coroutine.
            if (playerPosition == null)
            {
                yield break;
            }
            attractionTimer += Time.deltaTime;

            //based 0 - 1 nigga
            float t = Mathf.Clamp01(attractionTimer / attractionDuration);

            //passing it to processit with ease mode.
            float easedT = GetEasedT(t);

            // do the lerp based on t manipulated by ease mode.
            this.transform.position = Vector3.Lerp(CoinPosition, playerPosition.position - new Vector3(0, 1, 0), easedT);


            // This is the magic of coroutines.
            // 'yield return null' means "Pause here, wait for the next frame,
            // and then continue the loop."
            yield return null;
        }


        // And finally, destroy the coin GameObject and instantiate the object then add gold.
        Instantiate(SplashEffect, this.transform.position, quaternion.identity);
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddGold(goldValue);
        }
        SoundManager.PlaySound("CoinCollect", 1f);
        Destroy(this.gameObject);
    }


    /// <summary>
    /// Calculates the eased T value based on the selected EaseType.
    /// This is what creates the "ease mode" you wanted!
    /// </summary>
    private float GetEasedT(float t)
    {
        switch (easeType)
        {
            case EaseType.EaseIn:
                // Starts slow, speeds up (feels like a magnet)
                return t * t; // Quadratic Ease In

            case EaseType.EaseOut:
                // Starts fast, slows down (good for "arriving" gently)
                return 1 - (1 - t) * (1 - t); // Quadratic Ease Out

            case EaseType.EaseInOut:
                // Starts slow, speeds up in middle, slows down at end
                return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;

            case EaseType.Linear:
            default:
                // Constant speed, no easing
                return t;
        }
    }

    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if (collision.collider.CompareTag("Player"))
    //     {
    //         if (GoldManager.Instance != null)
    //         {
    //             GoldManager.Instance.AddGold(goldValue);
    //         }
    //         SoundManager.PlaySound("CoinCollect", 1f, this.transform.position);

    //         Destroy(gameObject);
    //     }
    // }

}
