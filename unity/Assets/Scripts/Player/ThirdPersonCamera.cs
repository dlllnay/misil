
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

    [SerializeField] private float recenterDelay = 1.5f;
    [SerializeField] private float recenterSpeed = 4f;

    private float yaw;
    private float pitch = 10f;
    private float lastControlTime;

    private void Start()
    {
        if (target != null)
        {
            yaw = target.eulerAngles.y;
            lastControlTime = Time.time;
            FollowTarget(1f);
        }
    }

    private void LateUpdate()
    {
        if (TempleMenu.BlocksGameplay || target == null)
            return;

        bool orbiting = Mouse.current != null && Mouse.current.middleButton.isPressed;
        bool aiming = Mouse.current != null && Mouse.current.rightButton.isPressed;
        Keyboard keyboard = Keyboard.current;
        bool moving = keyboard != null &&
            (keyboard.wKey.isPressed || keyboard.aKey.isPressed ||
             keyboard.sKey.isPressed || keyboard.dKey.isPressed);

        // Preserve free movement-facing rotation without rotating its camera basis.
        if (orbiting || aiming || moving)
            lastControlTime = Time.time;

        // Sadece orta fare tusuna basiliyken kamerayi elle dondur.
        if (orbiting)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            yaw += delta.x * mouseSensitivity;
            pitch -= delta.y * mouseSensitivity;

            pitch = Mathf.Clamp(pitch, -25f, 65f);
        }

        else if (!aiming && !moving && Time.time - lastControlTime >= recenterDelay)
        {
            float blend = 1f - Mathf.Exp(-recenterSpeed * Time.deltaTime);
            yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, blend);
            pitch = Mathf.Lerp(pitch, 10f, blend);
        }

        FollowTarget(smoothSpeed * Time.deltaTime);
    }

    private void FollowTarget(float blend)
    {

        Quaternion rotation =
            Quaternion.Euler(pitch, yaw, 0f);

        // Ranger ekranda solda, nisangah ortada kalir.
        Vector3 focusPoint =
            target.position +
            Vector3.up * height +
            rotation * Vector3.right * shoulderOffset;

        Vector3 desiredPosition =
            focusPoint -
            rotation * Vector3.forward * distance;

        // Kamerayi yumusakca takip ettir.
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            blend
        );

        transform.rotation = rotation;
    }
}
