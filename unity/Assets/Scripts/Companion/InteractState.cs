
public class InteractState : ICompanionState
{
    private readonly CompanionController _controller;
    private readonly SealStone _sealStone;

    public InteractState(
        CompanionController controller,
        SealStone sealStone)
    {
        _controller = controller;
        _sealStone = sealStone;
    }

    public string Name => "Interact";

    public void Enter()
    {
        _controller.Agent.ResetPath();

        _sealStone.Activate();

        _controller.EnterIdle();
    }

    public void Tick() { }

    public void Exit() { }
}
