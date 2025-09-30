using UnityEngine;
using System.Collections;

public class DissolveOnDestroy : MonoBehaviour
{
    [SerializeField] private float dissolveTime = 0.5f; // durasi hilang
    private SpriteRenderer[] renderers;

    void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void StartDissolve()
    {
        StartCoroutine(DissolveRoutine());
    }

    private IEnumerator DissolveRoutine()
    {
        float elapsed = 0f;

        while (elapsed < dissolveTime)
        {
            float t = elapsed / dissolveTime;
            float alpha = Mathf.Lerp(1f, 0f, t);

            foreach (var sr in renderers)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
