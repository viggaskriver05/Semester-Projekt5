using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerAudio : MonoBehaviour
{
    public AudioObject clipToPlay;

    // Delay before the first audio starts
    [Min(0f)]
    public float initialDelay = 2f;

    [System.Serializable]
    public class NextAudio
    {
        public AudioObject clip;

        [Min(0f)]
        public float delay = 1f;
    }

    public List<NextAudio> nextClips = new List<NextAudio>();

    public bool playOnce = true;

    private bool hasPlayed = false;
    private bool isPlaying = false;

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

        // Wait before playing the first audio
        yield return new WaitForSeconds(initialDelay);

        // Play the first audio
        if (clipToPlay != null && clipToPlay.clip != null)
        {
            Vocals.instance.Say(clipToPlay);

            yield return new WaitForSeconds(clipToPlay.clip.length);
        }

        // Play additional audio clips
        foreach (NextAudio next in nextClips)
        {
            if (next.clip == null || next.clip.clip == null)
                continue;

            // Wait before playing the next clip
            yield return new WaitForSeconds(next.delay);

            Vocals.instance.Say(next.clip);

            // Wait until the audio finishes
            yield return new WaitForSeconds(next.clip.clip.length);
        }

        isPlaying = false;
    }

    [ContextMenu("Test Audio Sequence")]
    private void TestSequence()
    {
        if (Application.isPlaying && !isPlaying)
            StartCoroutine(PlaySequence());
    }
}
