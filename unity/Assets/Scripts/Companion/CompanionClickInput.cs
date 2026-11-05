
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CompanionController))]
public class CompanionClickInput : MonoBehaviour
{
    private CompanionController _controller;
    private Camera _camera;
    private MuhurluKapi.Interaction.PlayerInteraction _interaction;

    private void Awake()
    {
        _controller = GetComponent<CompanionController>();
        _camera = Camera.main;
        _interaction = FindAnyObjectByType<MuhurluKapi.Interaction.PlayerInteraction>();
    }

    private void Update()
    {
        if (TempleMenu.BlocksGameplay || Mouse.current == null || Keyboard.current == null ||
            (_interaction != null && _interaction.IsDialogueOpen))
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
            NavMeshAgent agent = _controller.Agent;
            if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            {
                Debug.LogWarning("Nyra komutu reddedildi: NavMeshAgent etkin bir NavMesh uzerinde degil.", this);
                return;
            }

            // Reject walls and unreachable destinations before interrupting follow.
            var filter = new NavMeshQueryFilter
            {
                agentTypeID = agent.agentTypeID,
                areaMask = agent.areaMask
            };
            var path = new NavMeshPath();
            if (Vector3.Dot(hit.normal, Vector3.up) < 0.5f ||
                !NavMesh.SamplePosition(hit.point, out NavMeshHit destination, 1.5f, filter) ||
                !agent.CalculatePath(destination.position, path) ||
                path.status != NavMeshPathStatus.PathComplete)
            {
                Debug.LogWarning("Nyra komutu reddedildi: Ulasilabilir bir zemin noktasi secin.", this);
                return;
            }

            NyraFollowPlayer follow =
                GetComponent<NyraFollowPlayer>();

            if (follow != null)
            {
                follow.StopFollowing();
            }

            _controller.CommandMoveTo(destination.position);
        }

    }
}
