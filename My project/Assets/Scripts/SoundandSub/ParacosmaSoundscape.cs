using UnityEngine;
using System.Collections;

public class ParacosmaSoundscape : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource chaosSource;
    public AudioSource forestSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float chaosVolume = 0.7f;

    [Range(0f, 1f)]
    public float forestStartVolume = 0.05f;

    [Range(0f, 1f)]
    public float forestFinalVolume = 0.4f;

    [Header("Reveal Transition")]
    public bool startAutomatically = true;

    [Min(0f)]
    public float revealDelay = 0f;

    [Min(0f)]
    public float revealDuration = 8f;

    private bool revealStarted = false;

    void Start()
    {
        if (chaosSource == null || forestSource == null)
        {
            Debug.LogError("Soundscape: Missing AudioSource!");
            return;
        }

        chaosSource.playOnAwake = false;
        chaosSource.Stop();
        chaosSource.volume = chaosVolume;
        chaosSource.Play();

        forestSource.playOnAwake = false;
        forestSource.loop = true;
        forestSource.Stop();
        forestSource.volume = forestStartVolume;
        forestSource.Play();

        if (startAutomatically)
        {
            StartCoroutine(AutomaticReveal());
        }
    }

    private IEnumerator AutomaticReveal()
    {
        yield return new WaitForSeconds(revealDelay);
        BeginReveal();
    }

    [ContextMenu("Begin Reveal")]
    public void BeginReveal()
    {
        if (revealStarted)
            return;

        if (chaosSource == null || forestSource == null)
            return;

        revealStarted = true;
        StartCoroutine(FadeReveal());
    }

    private IEnumerator FadeReveal()
    {
        float elapsed = 0f;

        float startChaos = chaosSource.volume;
        float startForest = forestSource.volume;

        float duration = Mathf.Max(0.01f, revealDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            chaosSource.volume = Mathf.Lerp(
                startChaos,
                0f,
                smoothT
            );

            forestSource.volume = Mathf.Lerp(
                startForest,
                forestFinalVolume,
                smoothT
            );

            yield return null;
        }

        chaosSource.volume = 0f;
        chaosSource.Stop();

        forestSource.volume = forestFinalVolume;
    }
}
