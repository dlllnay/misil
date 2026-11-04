
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(RaycastObstacleSensor))]
public class CompanionController : MonoBehaviour
{
    private readonly CompanionStateMachine _machine =
        new CompanionStateMachine();

    public NavMeshAgent Agent { get; private set; }
    public IObstacleSensor Sensor { get; private set; }

    public string CurrentStateName =>
        _machine.Current?.Name ?? "-";

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Sensor = GetComponent<IObstacleSensor>();
    }

    private void Start()
    {
        EnterIdle();
    }

    private void Update()
    {
        _machine.Tick();
    }


    public void CommandMoveTo(Vector3 target, SealStone stone = null)
    {
        _machine.ChangeState(
            new MoveToTargetState(this, target, stone));
    }


    public void CommandInteract(SealStone stone)
    {
        if (stone == null) return;

        _machine.ChangeState(
            new InteractState(this, stone));
    }

    public void EnterIdle()
    {
        _machine.ChangeState(new IdleState(this));
    }
}
