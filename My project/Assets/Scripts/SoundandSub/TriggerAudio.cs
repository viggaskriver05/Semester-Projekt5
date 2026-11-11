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

    private bool hasPlayed = false;
    private bool isPlaying = false;

    private bool waitingForInput = false;
    private bool nextRequested = false;

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

    private IEnumerator PlaySequence()
    {
        isPlaying = true;
        hasPlayed = true;

        
        yield return new WaitForSeconds(initialDelay);

        
        if (clipToPlay != null && clipToPlay.clip != null)
        {
            Vocals.instance.Say(clipToPlay);

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

            
            Vocals.instance.Say(next.clip);

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
