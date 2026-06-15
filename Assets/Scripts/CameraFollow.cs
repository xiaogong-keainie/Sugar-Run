using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 6f;
    public float zoomSpeed = 4f;

    [Header("Framing")]
    public float groundY = 0f;
    public float minOrthoSize = 6f;
    public float maxOrthoSize = 18f;
    public float zoomPerUnitHeight = 0.45f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        float playerY = target.position.y;
        float playerHeight = Mathf.Max(0f, playerY - groundY);

        // Camera looks at midpoint between ground and player
        float lookY = (playerY + groundY) * 0.5f;

        // Ortho size grows with player height to keep both in frame
        float targetSize = Mathf.Clamp(
            minOrthoSize + playerHeight * zoomPerUnitHeight,
            minOrthoSize,
            maxOrthoSize
        );

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);

        float targetX = target.position.x;
        Vector3 desiredPos = new Vector3(targetX, lookY, -10f);
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }
}
