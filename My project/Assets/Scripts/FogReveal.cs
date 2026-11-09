using UnityEngine;
using System.Collections;

public class FogReveal : MonoBehaviour
{
    [Header("Fog Reveal Settings")]
    public float revealDuration = 8f;

    public float startFogDistance = 0.5f;

    public float finalFogStart = 15f;
    public float finalFogEnd = 70f;

    void Awake()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = Color.white;

        // Start with thick fog
        RenderSettings.fogStartDistance = 0f;
        RenderSettings.fogEndDistance = startFogDistance;
    }

    IEnumerator Start()
    {
        float timer = 0f;

        while (timer < revealDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / revealDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            RenderSettings.fogStartDistance =
                Mathf.Lerp(0f, finalFogStart, t);

            RenderSettings.fogEndDistance =
                Mathf.Lerp(startFogDistance, finalFogEnd, t);

            yield return null;
        }

        RenderSettings.fogStartDistance = finalFogStart;
        RenderSettings.fogEndDistance = finalFogEnd;
    }
}
