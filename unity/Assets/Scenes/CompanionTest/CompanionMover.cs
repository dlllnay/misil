using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class CompanionMover : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Camera _camera;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                MoveTo(hit.point);
            }
        }
    }

    public void MoveTo(Vector3 target)
    {
        _agent.SetDestination(target);
    }
}