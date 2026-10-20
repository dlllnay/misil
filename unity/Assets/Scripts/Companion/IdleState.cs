public class IdleState : ICompanionState
{
    private readonly CompanionController _controller;

    public IdleState(CompanionController controller)
    {
        _controller = controller;
    }

    public string Name => "Idle";

    public void Enter()
    {
        if (_controller.Agent.isOnNavMesh) _controller.Agent.ResetPath();
    }

    public void Tick() { }

    public void Exit() { }
}