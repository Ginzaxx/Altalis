using UnityEngine;
using System.Collections;

public class FadeAndDarkenOnMagmaContact : MonoBehaviour
{
    [Range(0f, 1f)]
    public float fadeAmount = 0.25f; // seberapa banyak opacity berkurang
    public float fadeSpeed = 2f;    // seberapa cepat perubahan transparansi dan warna

    private SpriteRenderer spriteRenderer;
    private Coroutine fadeCoroutine;

    // Warna target: #2A2525 (abu kehitaman)
    private Color targetDarkColor = new Color32(0x2A, 0x25, 0x25, 0xFF);

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning("FadeAndDarkenOnMagmaContact membutuhkan SpriteRenderer pada GameObject ini!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Magma"))
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeOutAndDarken());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Magma"))
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeOutAndDarken());
        }
    }

    private IEnumerator FadeOutAndDarken()
    {
        if (spriteRenderer == null)
            yield break;

        Color startColor = spriteRenderer.color;
        Color endColor = targetDarkColor;
        float startAlpha = startColor.a;
        float targetAlpha = Mathf.Clamp01(startAlpha - fadeAmount);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;

            // Interpolasi warna dan alpha
            Color currentColor = Color.Lerp(startColor, endColor, t);
            currentColor.a = Mathf.Lerp(startAlpha, targetAlpha, t);

            spriteRenderer.color = currentColor;
            yield return null;
        }
    }
}
