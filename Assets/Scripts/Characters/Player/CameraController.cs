using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [SerializeField] float yaw = 45f;
    [SerializeField] float pitch = 67.5f;
    [SerializeField] float distance = 60f;
    [SerializeField] float panThreshold = 8f;

    private Transform target;
    private Vector3 focus;
    private Vector3 velocity;
    private Vector3 lastTargetPosition;
    private Vector3 lastMousePosition;
    private bool following = true;
    private bool pannedThisGesture;
    private readonly float smoothTime = 0.1f;

    public bool PannedThisGesture => pannedThisGesture;

    private void OnValidate()
    {
        ApplyFixedRotation();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        following = true;
        if (target == null)
            return;

        focus = target.position;
        lastTargetPosition = target.position;
        ApplyFixedRotation();
    }

    private void Update()
    {
        HandlePanInput();
        Follow();
    }

    private void HandlePanInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pannedThisGesture = false;
            lastMousePosition = Input.mousePosition;
        }

        if (!Input.GetMouseButton(0) || IsPointerOverUI() || TargetIsMoving())
            return;

        Vector3 mouse = Input.mousePosition;
        Vector3 delta = mouse - lastMousePosition;
        lastMousePosition = mouse;

        if (!pannedThisGesture)
        {
            if (delta.sqrMagnitude < panThreshold * panThreshold)
                return;
            pannedThisGesture = true;
            following = false;
        }

        Pan(delta);
    }

    private void Follow()
    {
        if (target == null)
            return;

        if (TargetIsMoving())
            following = true;

        lastTargetPosition = target.position;
        ApplyFixedRotation();

        if (following)
            focus = target.position;

        Vector3 desired = focus + Offset;
        if (following)
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        else
            transform.position = desired;
    }

    private void Pan(Vector3 screenDelta)
    {
        Camera cam = GetComponent<Camera>();
        float unitsPerPixel = cam.orthographic
            ? 2f * cam.orthographicSize / Screen.height
            : 0.2f;

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 forward = Vector3.Cross(right, Vector3.up);

        focus += (-right * screenDelta.x - forward * screenDelta.y) * unitsPerPixel;
        focus.y = target != null ? target.position.y : focus.y;
    }

    private void ApplyFixedRotation()
    {
        Vector3 offset = Offset;
        if (offset.sqrMagnitude < 0.0001f)
            return;
        transform.rotation = Quaternion.LookRotation(-offset);
    }

    private Vector3 Offset
    {
        get
        {
            float pitchRad = pitch * Mathf.Deg2Rad;
            float yawRad = yaw * Mathf.Deg2Rad;
            float horizontal = Mathf.Cos(pitchRad) * distance;
            return new Vector3(
                -Mathf.Sin(yawRad) * horizontal,
                Mathf.Sin(pitchRad) * distance,
                -Mathf.Cos(yawRad) * horizontal);
        }
    }

    private bool TargetIsMoving()
    {
        if (target == null)
            return false;
        return (target.position - lastTargetPosition).sqrMagnitude > 0.0001f;
    }

    private static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
