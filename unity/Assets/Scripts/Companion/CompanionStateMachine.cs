public class CompanionStateMachine
{
    public ICompanionState Current { get; private set; }

    public void ChangeState(ICompanionState next)
    {
        Current?.Exit();
        Current = next;
        Current.Enter();
    }

    public void Tick()
    {
        Current?.Tick();
    }
}