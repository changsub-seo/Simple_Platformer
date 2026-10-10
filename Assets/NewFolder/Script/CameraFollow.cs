using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    [Min(0f)] public float smoothTime = 0.25f;
    public CameraBounds cameraBounds;

    private Vector3 velocity = Vector3.zero;
    private Camera attachedCamera;

    private void Awake()
    {
        attachedCamera = GetComponent<Camera>();
        if (cameraBounds == null) cameraBounds = FindObjectOfType<CameraBounds>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position, targetPosition, ref velocity, smoothTime);

        if (cameraBounds == null)
        {
            transform.position = smoothedPosition;
            return;
        }

        Vector3 constrainedPosition = cameraBounds.ResolveCameraPosition(
            transform.position, smoothedPosition, attachedCamera);
        if (!Mathf.Approximately(constrainedPosition.x, smoothedPosition.x)) velocity.x = 0f;
        if (!Mathf.Approximately(constrainedPosition.y, smoothedPosition.y)) velocity.y = 0f;
        transform.position = constrainedPosition;
    }
}
