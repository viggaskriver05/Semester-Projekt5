using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepAudio : MonoBehaviour
{
    [Header("Player")]
    public Transform xrOrigin;

    [Header("Footstep Sounds")]
    public AudioClip[] footstepClips;

    [Range(0f, 1f)]
    public float footstepVolume = 0.5f;

    [Header("Walking Settings")]
    [Min(0.1f)]
    public float stepInterval = 0.6f;

    [Min(0f)]
    public float movementThreshold = 0.08f;

    [Header("Pitch Variation")]
    [Range(0.5f, 2f)]
    public float minPitch = 0.95f;

    [Range(0.5f, 2f)]
    public float maxPitch = 1.05f;

    private AudioSource audioSource;
    private Vector3 lastPosition;

    private float stepTimer;
    private bool wasMoving;

    private int lastClipIndex = -1;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;

        if (xrOrigin == null)
        {
            Debug.LogError(
                "FootstepAudio: XR Origin is missing!",
                this
            );

            enabled = false;
            return;
        }

        lastPosition = xrOrigin.position;
    }

    void Update()
    {
        if (Time.deltaTime <= 0f)
            return;

        Vector3 currentPosition = xrOrigin.position;

        Vector3 movement = currentPosition - lastPosition;

        movement.y = 0f;

        lastPosition = currentPosition;

        float speed = movement.magnitude / Time.deltaTime;

        bool isMoving = speed > movementThreshold;

        if (!isMoving)
        {
            wasMoving = false;
            stepTimer = 0f;
            return;
        }

        if (!wasMoving)
        {
            PlayRandomFootstep();

            wasMoving = true;
            stepTimer = stepInterval;

            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayRandomFootstep();
            stepTimer = stepInterval;
        }
    }

    private void PlayRandomFootstep()
    {
        if (footstepClips == null ||
            footstepClips.Length == 0)
            return;

        int index = Random.Range(0, footstepClips.Length);

        if (footstepClips.Length > 1 &&
            index == lastClipIndex)
        {
            int offset = Random.Range(
                1,
                footstepClips.Length
            );

            index = (index + offset) % footstepClips.Length;
        }

        AudioClip selectedClip = footstepClips[index];

        if (selectedClip == null)
            return;

        lastClipIndex = index;

        audioSource.pitch = Random.Range(
            Mathf.Min(minPitch, maxPitch),
            Mathf.Max(minPitch, maxPitch)
        );

        audioSource.PlayOneShot(
            selectedClip,
            footstepVolume
        );
    }
}
