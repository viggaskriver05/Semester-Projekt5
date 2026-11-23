using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class FlowerSound : MonoBehaviour
{
    [Header("Sound Clips")]
    public AudioClip[] soundClips;

    [Header("Playback")]
    public bool startAutomatically = false;

    [Range(0f, 1f)]
    public float maxVolume = 0.6f;

    [Header("Random Timing")]
    public float minInitialDelay = 0f;
    public float maxInitialDelay = 4f;

    public float minPause = 2f;
    public float maxPause = 6f;

    [Header("Pitch Variation")]
    [Range(0.5f, 2f)]
    public float minPitch = 0.95f;

    [Range(0.5f, 2f)]
    public float maxPitch = 1.05f;

    [Header("Fading")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.5f;

    private AudioSource audioSource;
    private Coroutine singingCoroutine;
    private Coroutine fadeCoroutine;
    private bool isSinging = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
        audioSource.dopplerLevel = 0f;
        audioSource.volume = 0f;
    }

    void Start()
    {
        if (startAutomatically)
            StartSinging();
    }

    [ContextMenu("Start Singing")]
    public void StartSinging()
    {
        if (isSinging)
            return;

        if (soundClips == null || soundClips.Length == 0)
        {
            Debug.LogWarning("No flower sounds assigned!", this);
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        audioSource.Stop();
        audioSource.volume = 0f;

        isSinging = true;
        singingCoroutine = StartCoroutine(SingingLoop());
    }

    [ContextMenu("Stop Singing")]
    public void StopSinging()
    {
        if (!isSinging)
            return;

        isSinging = false;

        if (singingCoroutine != null)
        {
            StopCoroutine(singingCoroutine);
            singingCoroutine = null;
        }

        if (audioSource.isPlaying)
            fadeCoroutine = StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator SingingLoop()
    {
        float initialDelay = Random.Range(
            Mathf.Min(minInitialDelay, maxInitialDelay),
            Mathf.Max(minInitialDelay, maxInitialDelay)
        );

        yield return new WaitForSeconds(Mathf.Max(0f, initialDelay));

        while (isSinging)
        {
            AudioClip clip = soundClips[
                Random.Range(0, soundClips.Length)
            ];

            if (clip != null)
            {
                audioSource.pitch = Random.Range(
                    Mathf.Min(minPitch, maxPitch),
                    Mathf.Max(minPitch, maxPitch)
                );

                audioSource.clip = clip;
                audioSource.volume = 0f;
                audioSource.Play();

                float duration = clip.length / audioSource.pitch;

                float fadeIn = Mathf.Min(
                    Mathf.Max(0f, fadeInDuration),
                    duration * 0.45f
                );

                float fadeOut = Mathf.Min(
                    Mathf.Max(0f, fadeOutDuration),
                    duration * 0.45f
                );

                float elapsed = 0f;

                while (elapsed < duration && audioSource.isPlaying)
                {
                    float inFactor = fadeIn > 0f
                        ? Mathf.Clamp01(elapsed / fadeIn)
                        : 1f;

                    float outFactor = fadeOut > 0f
                        ? Mathf.Clamp01((duration - elapsed) / fadeOut)
                        : 1f;

                    audioSource.volume = maxVolume *
                        Mathf.Min(inFactor, outFactor);

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                audioSource.Stop();
                audioSource.volume = 0f;
            }

            float pause = Random.Range(
                Mathf.Min(minPause, maxPause),
                Mathf.Max(minPause, maxPause)
            );

            yield return new WaitForSeconds(Mathf.Max(0f, pause));
        }

        singingCoroutine = null;
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVolume = audioSource.volume;
        float duration = Mathf.Max(0.01f, fadeOutDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            audioSource.volume = Mathf.Lerp(
                startVolume, 0f, t
            );

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = 0f;
        fadeCoroutine = null;
    }

    void OnDisable()
    {
        isSinging = false;
        StopAllCoroutines();

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.volume = 0f;
        }

        singingCoroutine = null;
        fadeCoroutine = null;
    }
}
