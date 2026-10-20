using UnityEngine;

public class MoveToTargetState : ICompanionState
{
    private readonly CompanionController _controller;
    private readonly Vector3 _target;
    private float _nextLogTime;

    public MoveToTargetState(CompanionController controller, Vector3 target)
    {
        _controller = controller;
        _target = target;
    }

    public string Name => "MoveToTarget";

    public void Enter()
    {
        _controller.Agent.SetDestination(_target);
    }

    public void Tick()
    {
        ObstacleReading reading = _controller.Sensor.Sense();
        if (reading.HasObstacle && reading.Distance < 1.5f && Time.time > _nextLogTime)
        {
            _nextLogTime = Time.time + 0.5f;
            Debug.Log($"Engel algilandi: mesafe={reading.Distance:F2}, aci={reading.AngleDegrees:F0}");
        }

        var agent = _controller.Agent;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            _controller.EnterIdle();
        }
    }

    public void Exit() { }
}