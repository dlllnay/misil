
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private float distance = 4f;
    [SerializeField] private float height = 1.5f;
    [SerializeField] private float shoulderOffset = 1.1f;

    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float smoothSpeed = 12f;

    private float yaw;
    private float pitch = 10f;

    private void Start()
    {
        if (target != null)
            yaw = target.eulerAngles.y;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (Mouse.current != null &&
            Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            yaw += delta.x * mouseSensitivity;
            pitch -= delta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -25f, 65f);
        }

        Quaternion rotation =
            Quaternion.Euler(pitch, yaw, 0f);

        // Kameranin baktigi nokta karakterin saginda.
        // Bu sayede Ranger ekranda solda kalir.
        Vector3 focusPoint =
            target.position +
            Vector3.up * height +
            rotation * Vector3.right * shoulderOffset;

        Vector3 desiredPosition =
            focusPoint -
            rotation * Vector3.forward * distance;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.rotation = rotation;
    }
}
