using UnityEngine;

public class CameraRelativeUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform targetCamera;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, 1.0f); // 1 meter in front
    [SerializeField] private bool followRotation = true;

    void Start()
    {
        if (targetCamera == null && Camera.main != null)
        {
            targetCamera = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (targetCamera != null)
        {
            transform.position = targetCamera.TransformPoint(offset);

            if (followRotation)
            {
                transform.rotation = targetCamera.rotation;
            }
        }
    }
}