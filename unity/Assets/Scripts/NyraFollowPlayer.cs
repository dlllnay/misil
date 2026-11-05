
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using MuhurluKapi.Interaction;

[RequireComponent(typeof(CompanionController))]
public class NyraFollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followDistance = 3f;
    [SerializeField] private float updateInterval = 0.6f;
    [SerializeField] private float targetChangeDistance = 1f;

    private CompanionController controller;
    private NavMeshAgent agent;
    private PlayerInteraction playerInteraction;

    private bool isFollowing = true;
    private float nextUpdate;
    private Vector3 lastTargetPosition;
    private bool hasTarget;

    private void Awake()
    {
        controller = GetComponent<CompanionController>();
        agent = GetComponent<NavMeshAgent>();
        if (player != null)
            playerInteraction = player.GetComponent<PlayerInteraction>();
    }

    private void Update()
    {
        if (TempleMenu.BlocksGameplay) return;

        if (Keyboard.current != null &&
            Keyboard.current.gKey.wasPressedThisFrame &&
            (playerInteraction == null || !playerInteraction.IsDialogueOpen) &&
            !PlayerInteraction.IsTextInputFocused())
        {
            isFollowing = !isFollowing;
            hasTarget = false;

            Debug.Log(isFollowing
                ? "Nyra takip modunda."
                : "Nyra serbest komut modunda.");
        }

        if (!isFollowing || player == null)
            return;

        if (Time.time < nextUpdate)
            return;

        nextUpdate = Time.time + updateInterval;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= followDistance)
        {
            // Yakinindayken yeni hareket emri gonderme.
            return;
        }

        Vector3 targetPosition =
            player.position - player.forward * 1.5f;

        if (hasTarget &&
            Vector3.Distance(
                targetPosition,
                lastTargetPosition
            ) < targetChangeDistance &&
            agent != null &&
            agent.hasPath)
        {
            return;
        }

        if (NavMesh.SamplePosition(
            targetPosition,
            out NavMeshHit hit,
            2f,
            NavMesh.AllAreas))
        {
            controller.CommandMoveTo(hit.position);
            lastTargetPosition = targetPosition;
            hasTarget = true;
        }
    }

    public void StopFollowing()
    {
        isFollowing = false;
        hasTarget = false;
        Debug.Log("Nyra manuel komut bekliyor.");
    }
}
