
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 6f;
    [SerializeField] private float height = 1.5f;
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float smoothSpeed = 12f;

    private float yaw;
    private float pitch = 15f;

    private void Start()
    {
        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }
    }

    private void LateUpdate()
    {
        if (target == null || Mouse.current == null)
            return;

        // Sadece sag fare tusuna basiliyken kamerayi dondur.
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            yaw += mouseDelta.x * mouseSensitivity;
            pitch -= mouseDelta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -25f, 65f);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 focusPoint =
            target.position + Vector3.up * height;

        Vector3 desiredPosition =
            focusPoint - rotation * Vector3.forward * distance;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.LookRotation(
            focusPoint - transform.position
        );
    }
}
