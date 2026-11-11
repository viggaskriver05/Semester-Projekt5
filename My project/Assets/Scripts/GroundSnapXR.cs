
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GroundSnapXR : MonoBehaviour
{
    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float rayStartHeight = 0.3f;
    public float maxSnapDistance = 0.4f;

    [Header("Movement")]
    public float snapSpeed = 1.0f;
    public float groundOffset = 0.02f;

    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void LateUpdate()
    {
        if (!controller.enabled)
            return;

        Vector3 feetPosition = new Vector3(
            controller.bounds.center.x,
            controller.bounds.min.y,
            controller.bounds.center.z
        );

        Vector3 rayOrigin = feetPosition +
                            Vector3.up * rayStartHeight;

        float rayLength = rayStartHeight + maxSnapDistance;

        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            rayLength,
            groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            float distanceToGround =
                feetPosition.y - hit.point.y - groundOffset;

            if (distanceToGround > 0f &&
                distanceToGround <= maxSnapDistance)
            {
                float movement = Mathf.Min(
                    distanceToGround,
                    snapSpeed * Time.deltaTime
                );

                controller.Move(Vector3.down * movement);
            }
        }
    }
}
