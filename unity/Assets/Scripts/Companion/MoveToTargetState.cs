
using UnityEngine;
using UnityEngine.AI;

public class MoveToTargetState : ICompanionState
{
    private readonly CompanionController _controller;
    private readonly Vector3 _target;
    private readonly SealStone _sealStone;

    private float _nextLogTime;

    public MoveToTargetState(
        CompanionController controller,
        Vector3 target,
        SealStone sealStone = null)
    {
        _controller = controller;
        _target = target;
        _sealStone = sealStone;
    }

    public string Name => "MoveToTarget";

    public void Enter()
    {
        if (_controller.Agent.isOnNavMesh)
        {
            _controller.Agent.SetDestination(_target);
        }
    }

    public void Tick()
    {
        ObstacleReading reading = _controller.Sensor.Sense();

        if (reading.HasObstacle &&
            reading.Distance < 1.5f &&
            Time.time > _nextLogTime)
        {
            _nextLogTime = Time.time + 0.5f;

            Debug.Log(
                $"Engel algilandi: mesafe={reading.Distance:F2}");
        }

        NavMeshAgent agent = _controller.Agent;

        if (!agent.isOnNavMesh ||
            agent.pathPending ||
            !agent.hasPath)
            return;

        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            return;

        float arrivalDistance =
            Mathf.Max(agent.stoppingDistance, 0.6f);

        if (agent.remainingDistance <= arrivalDistance)
        {
            if (_sealStone != null)
            {
                _controller.CommandInteract(_sealStone);
            }
            else
            {
                _controller.EnterIdle();
            }
        }
    }

    public void Exit() { }
}
