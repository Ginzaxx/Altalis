using UnityEngine;
using System.Collections;

public class ExpandOnSpawn : MonoBehaviour
{
    [SerializeField] private float expandTime = 0.6f;       // duration of the animation
    [SerializeField] private float overshoot = 1.2f;        // how much to "pop" past full size
    [SerializeField] private AnimationCurve easeCurve;      // curve for smoother effect
    private SpriteRenderer[] renderers;
    private Vector3[] originalScales;

    void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        originalScales = new Vector3[renderers.Length];




    }

    public void StartExpand()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            var sr = renderers[i];
            if (sr != null)
            {
                originalScales[i] = sr.transform.localScale;
                sr.transform.localScale = Vector3.zero; // start from zero scale
            }
        }

        // Default curve if not assigned in inspector
        if (easeCurve == null || easeCurve.length == 0)
        {
            easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
        StartCoroutine(ExpandRoutine());
    }

    private IEnumerator ExpandRoutine()
    {
        float elapsed = 0f;

        while (elapsed < expandTime)
        {
            float t = elapsed / expandTime;
            float curveValue = easeCurve.Evaluate(t);

            // overshoot (spring-like bounce)
            float scaleFactor = Mathf.Lerp(0f, overshoot, curveValue);

            for (int i = 0; i < renderers.Length; i++)
            {
                var sr = renderers[i];
                if (sr != null)
                    sr.transform.localScale = originalScales[i] * scaleFactor;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final scale is perfect
        for (int i = 0; i < renderers.Length; i++)
        {
            var sr = renderers[i];
            if (sr != null)
                sr.transform.localScale = originalScales[i];
        }
    }
}
