using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MovingMusic : MonoBehaviour
{
    [Header("References")]
    public Transform playerHead;

    [Header("Trigger")]
    [Min(0f)]
    public float startDelay = 0f;

    [Header("Movement")]
    public float startHeight = 8f;
    public float endHeight = 0.5f;

    [Min(0f)]
    public float orbitRadius = 2.5f;

    [Min(0f)]
    public float orbitTurns = 1f;

    [Min(0.1f)]
    public float movementDuration = 12f;

    [Header("Audio")]
    [Range(0f, 1f)]
    public float targetVolume = 0.7f;

    [Min(0f)]
    public float fadeInDuration = 1f;

    [Min(0f)]
    public float fadeOutDuration = 2f;

    private AudioSource audioSource;
    private bool hasTriggered = false;
    private bool isActive = false;

    private Vector3 initialForward;
    private Vector3 initialRight;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
        audioSource.dopplerLevel = 0f;
        audioSource.pitch = 1f;
        audioSource.volume = 0f;
    }

    [ContextMenu("Test - Play Music")]
    public void PlayMusic()
    {
        if (hasTriggered)
            return;

        if (playerHead == null || audioSource.clip == null)
        {
            Debug.LogWarning(
                "MovingMusic: Missing playerHead or AudioClip!",
                this
            );
            return;
        }

        hasTriggered = true;

        if (startDelay > 0f)
            StartCoroutine(DelayedStart());
        else
            BeginMusic();
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(startDelay);
        BeginMusic();
    }

    private void BeginMusic()
    {
        initialForward = Vector3.ProjectOnPlane(
            playerHead.forward,
            Vector3.up
        ).normalized;

        if (initialForward.sqrMagnitude < 0.001f)
            initialForward = Vector3.forward;

        initialRight = Vector3.Cross(
            Vector3.up,
            initialForward
        ).normalized;

        transform.position =
            playerHead.position + Vector3.up * startHeight;

        audioSource.volume = 0f;
        audioSource.Play();

        isActive = true;
    }

    void Update()
    {
        if (!isActive)
            return;

        if (!audioSource.isPlaying)
        {
            isActive = false;
            return;
        }

        float clipDuration = audioSource.clip.length;
        float currentTime = audioSource.time;

        float duration = Mathf.Max(
            0.01f,
            Mathf.Min(movementDuration, clipDuration)
        );

        float t = Mathf.Clamp01(currentTime / duration);

        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        float height = Mathf.Lerp(
            startHeight,
            endHeight,
            smoothT
        );

        float radius = orbitRadius *
            Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 2f));

        float angle = t * orbitTurns * Mathf.PI * 2f;

        Vector3 horizontalOffset =
            initialRight * Mathf.Cos(angle) * radius +
            initialForward * Mathf.Sin(angle) * radius;

        transform.position =
            playerHead.position +
            Vector3.up * height +
            horizontalOffset;

        float fadeIn = fadeInDuration > 0f
            ? Mathf.Clamp01(currentTime / fadeInDuration)
            : 1f;

        float remaining = clipDuration - currentTime;

        float fadeOut = fadeOutDuration > 0f
            ? Mathf.Clamp01(remaining / fadeOutDuration)
            : 1f;

        audioSource.volume =
            targetVolume * Mathf.Min(fadeIn, fadeOut);
    }
}
