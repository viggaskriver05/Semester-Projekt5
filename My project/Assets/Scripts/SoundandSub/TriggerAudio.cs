using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class TriggerAudio : MonoBehaviour
{
    public AudioObject clipToPlay;

    [Min(0f)]
    public float initialDelay = 2f;

    [System.Serializable]
    public class NextAudio
    {
        public AudioObject clip;
    }

    public List<NextAudio> nextClips = new List<NextAudio>();

    public bool playOnce = true;

    [Header("VR Controls")]
    public InputActionReference nextLineAction;

    [Header("Moving Music")]
    public MovingMusic movingMusic;
    public AudioObject musicTriggerClip;

    [Header("Choir Ambience")]
    public AudioSource choirSource;
    public AudioObject choirTriggerClip;

    [Range(0f, 1f)]
    public float choirVolume = 0.5f;

    [Min(0f)]
    public float choirFadeIn = 2f;

    [Min(0f)]
    public float choirFadeOut = 2f;

    private bool hasPlayed = false;
    private bool isPlaying = false;

    private bool waitingForInput = false;
    private bool nextRequested = false;

    private bool choirHasPlayed = false;

    private void Awake()
    {
        if (choirSource != null)
        {
            choirSource.playOnAwake = false;
            choirSource.loop = false;
            choirSource.spatialBlend = 0f;
            choirSource.volume = 0f;
        }
    }

    private void OnEnable()
    {
        if (nextLineAction != null && nextLineAction.action != null)
        {
            nextLineAction.action.performed += OnNextLinePressed;
        }
    }

    private void OnDisable()
    {
        if (nextLineAction != null && nextLineAction.action != null)
        {
            nextLineAction.action.performed -= OnNextLinePressed;
        }
    }

    private void OnNextLinePressed(InputAction.CallbackContext context)
    {
        if (waitingForInput)
        {
            nextRequested = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (isPlaying || (playOnce && hasPlayed))
            return;

        StartCoroutine(PlaySequence());
    }

    private void PlayNarration(AudioObject audio)
    {
        Vocals.instance.Say(audio);

        if (audio == musicTriggerClip && movingMusic != null)
        {
            movingMusic.PlayMusic();
        }

        if (audio == choirTriggerClip &&
            choirSource != null &&
            choirSource.clip != null &&
            !choirHasPlayed)
        {
            choirHasPlayed = true;
            StartCoroutine(PlayChoir());
        }
    }

    private IEnumerator PlayChoir()
    {
        choirSource.volume = 0f;
        choirSource.Play();

        float clipLength = choirSource.clip.length;

        while (choirSource.isPlaying)
        {
            float currentTime = choirSource.time;
            float remainingTime = clipLength - currentTime;

            float fadeIn = choirFadeIn > 0f
                ? Mathf.Clamp01(currentTime / choirFadeIn)
                : 1f;

            float fadeOut = choirFadeOut > 0f
                ? Mathf.Clamp01(remainingTime / choirFadeOut)
                : 1f;

            choirSource.volume =
                choirVolume * Mathf.Min(fadeIn, fadeOut);

            yield return null;
        }

        choirSource.volume = 0f;
    }

    private IEnumerator PlaySequence()
    {
        isPlaying = true;
        hasPlayed = true;

        yield return new WaitForSeconds(initialDelay);

        if (clipToPlay != null && clipToPlay.clip != null)
        {
            PlayNarration(clipToPlay);

            yield return new WaitForSeconds(clipToPlay.clip.length);
        }

        foreach (NextAudio next in nextClips)
        {
            if (next.clip == null || next.clip.clip == null)
                continue;

            nextRequested = false;
            waitingForInput = true;

            yield return new WaitUntil(() => nextRequested);

            waitingForInput = false;

            PlayNarration(next.clip);

            yield return new WaitForSeconds(next.clip.clip.length);
        }

        waitingForInput = false;
        isPlaying = false;
    }

    [ContextMenu("Test Audio Sequence")]
    private void TestSequence()
    {
        if (Application.isPlaying && !isPlaying)
            StartCoroutine(PlaySequence());
    }
}

