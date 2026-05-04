using UnityEngine;

public class CameraRelativeUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform targetCamera;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, 1.0f); // 1 meter in front
    [SerializeField] private bool followRotation = true;

    void Start()
    {
        // If no camera is assigned, default to the Main Camera
        if (targetCamera == null && Camera.main != null)
        {
            targetCamera = Camera.main.transform;
        }
    }

    // LateUpdate is critical for UI following cameras to prevent jitter
    void LateUpdate()
    {
        if (targetCamera == null) return;

        // 1. Position the UI relative to the camera's current orientation
        // transform.TransformPoint converts our local offset into world space
        transform.position = targetCamera.TransformPoint(offset);

        // 2. Make the UI face the same way as the camera
        if (followRotation)
        {
            transform.rotation = targetCamera.rotation;
        }
    }
}