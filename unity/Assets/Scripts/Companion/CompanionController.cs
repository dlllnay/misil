using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(RaycastObstacleSensor))]
public class CompanionController : MonoBehaviour
{
    private readonly CompanionStateMachine _machine = new CompanionStateMachine();

    public NavMeshAgent Agent { get; private set; }
    public IObstacleSensor Sensor { get; private set; }
    public string CurrentStateName => _machine.Current?.Name ?? "-";

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

    public void CommandMoveTo(Vector3 target)
    {
        _machine.ChangeState(new MoveToTargetState(this, target));
    }

    public void EnterIdle()
    {
        _machine.ChangeState(new IdleState(this));
    }
}