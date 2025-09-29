// using UnityEngine;

// public class ParallaxBackground : MonoBehaviour
// {
//     [Header("Parallax Settings")]
//     [Range(0f, 1f)]
//     public float parallaxEffect = 0.5f; // lower = further, higher = close

//     [Header("Infinite Scrolling")]
//     public bool infiniteHorizontal = true;
//     public bool infiniteVertical = false;

//     private Transform cam;
//     private Vector3 lastCamPos;
//     private float textureUnitSizeX;
//     private float textureUnitSizeY;

//     void Start()
//     {
//         cam = Camera.main.transform;
//         lastCamPos = cam.position;

//         // Get sprite size (assumes sprite renderer with a tiled/loopable texture)
//         SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
//         if (spriteRenderer != null && spriteRenderer.sprite != null)
//         {
//             Texture2D texture = spriteRenderer.sprite.texture;
//             textureUnitSizeX = spriteRenderer.bounds.size.x;
//             textureUnitSizeY = spriteRenderer.bounds.size.y;
//         }
//     }

//     void LateUpdate()
//     {
//         // Normal parallax movement
//         Vector3 deltaMovement = cam.position - lastCamPos;
//         transform.position += new Vector3(deltaMovement.x * parallaxEffect, deltaMovement.y * parallaxEffect, 0);
//         lastCamPos = cam.position;

//         // Infinite scrolling
//         if (infiniteHorizontal && Mathf.Abs(cam.position.x - transform.position.x) >= textureUnitSizeX)
//         {
//             float offsetX = (cam.position.x - transform.position.x) % textureUnitSizeX;
//             transform.position = new Vector3(cam.position.x + offsetX, transform.position.y, transform.position.z);
//         }

//         if (infiniteVertical && Mathf.Abs(cam.position.y - transform.position.y) >= textureUnitSizeY)
//         {
//             float offsetY = (cam.position.y - transform.position.y) % textureUnitSizeY;
//             transform.position = new Vector3(transform.position.x, cam.position.y + offsetY, transform.position.z);
//         }
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    Transform cam; //Main Camera
    Vector3 camStartPos;
    float distance; //jarak antara start camera posisi dan current posisi

    GameObject[] backgrounds;
    Material[] mat;
    float[] backSpeed;

    float farthestBack;

    [Range(0.01f, 0.05f)]
    public float parallaxSpeed;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;

        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            mat[i] = backgrounds[i].GetComponent<Renderer>().material;
        }

        BackSpeedCalculate(backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        for (int i = 0; i < backCount; i++) //find the farthest background
        {
            if ((backgrounds[i].transform.position.z - cam.position.z) > farthestBack)
            {
                farthestBack = backgrounds[i].transform.position.z - cam.position.z;
            }
        }

        for (int i = 0; i < backCount; i++) //set the speed of bacground
        {
            backSpeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
        }
    }

    private void LateUpdate()
    {
        distance = cam.position.x - camStartPos.x;
        transform.position = new Vector3(cam.position.x, transform.position.y, 0);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;
            mat[i].SetTextureOffset("_MainTex", new Vector2(distance, 0) * speed);
        }
    }


}