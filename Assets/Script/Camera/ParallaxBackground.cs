using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Range(0f, 1f)]
    public float parallaxEffect = 0.5f; // lower = further, higher = close

    [Header("Infinite Scrolling")]
    public bool infiniteHorizontal = true;
    public bool infiniteVertical = false;

    private Transform cam;
    private Vector3 lastCamPos;
    private float textureUnitSizeX;
    private float textureUnitSizeY;

    void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;

        // Get sprite size (assumes sprite renderer with a tiled/loopable texture)
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Texture2D texture = spriteRenderer.sprite.texture;
            textureUnitSizeX = spriteRenderer.bounds.size.x;
            textureUnitSizeY = spriteRenderer.bounds.size.y;
        }
    }

    void LateUpdate()
    {
        // Normal parallax movement
        Vector3 deltaMovement = cam.position - lastCamPos;
        transform.position += new Vector3(deltaMovement.x * parallaxEffect, deltaMovement.y * parallaxEffect, 0);
        lastCamPos = cam.position;

        // Infinite scrolling
        if (infiniteHorizontal && Mathf.Abs(cam.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offsetX = (cam.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(cam.position.x + offsetX, transform.position.y, transform.position.z);
        }

        if (infiniteVertical && Mathf.Abs(cam.position.y - transform.position.y) >= textureUnitSizeY)
        {
            float offsetY = (cam.position.y - transform.position.y) % textureUnitSizeY;
            transform.position = new Vector3(transform.position.x, cam.position.y + offsetY, transform.position.z);
        }
    }
}
