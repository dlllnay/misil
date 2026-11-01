
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CompanionController))]
public class CompanionClickInput : MonoBehaviour
{
    private CompanionController _controller;
    private Camera _camera;

    private void Awake()
    {
        _controller = GetComponent<CompanionController>();
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null || Keyboard.current == null)
            return;

        bool shiftPressed =
            Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.rightShiftKey.isPressed;

        if (!shiftPressed ||
            !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (_camera == null)
            return;

        Ray ray = _camera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );


        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            NyraFollowPlayer follow =
                GetComponent<NyraFollowPlayer>();

            if (follow != null)
            {
                follow.StopFollowing();
            }

            _controller.CommandMoveTo(hit.point);
        }

    }
}
