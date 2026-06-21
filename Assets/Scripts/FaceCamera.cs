using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 direction =
            transform.position - targetCamera.transform.position;

        transform.rotation = Quaternion.LookRotation(
            direction,
            Vector3.up
        );
    }
}