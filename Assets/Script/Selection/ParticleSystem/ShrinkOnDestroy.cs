using UnityEngine;
using System.Collections;

public class ShrinkOnDestroy : MonoBehaviour
{
    [SerializeField] private float shrinkTime = 0.6f;     // total shrink duration
    [SerializeField] private float springStrength = 1.3f; // how strong the "pop" at start is (1 = none)
    private SpriteRenderer[] renderers;
    private Vector3[] originalScales;

    void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        originalScales = new Vector3[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalScales[i] = renderers[i].transform.localScale;
        }
    }

    public void StartShrink()
    {
        StartCoroutine(ShrinkRoutine());
    }

    private IEnumerator ShrinkRoutine()
    {
        float elapsed = 0f;

        // optional spring pop-up at start
        foreach (var sr in renderers)
        {
            if (sr != null)
                sr.transform.localScale *= springStrength;
        }

        while (elapsed < shrinkTime)
        {
            float t = elapsed / shrinkTime;
            // smooth shrink curve with ease-out (a bit of spring-like feel)
            float scaleFactor = Mathf.Lerp(1f, 0f, Mathf.SmoothStep(0f, 1f, t));

            for (int i = 0; i < renderers.Length; i++)
            {
                var sr = renderers[i];
                if (sr != null)
                    sr.transform.localScale = originalScales[i] * scaleFactor;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
