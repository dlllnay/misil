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
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            _controller.CommandMoveTo(hit.point);
        }
    }
}