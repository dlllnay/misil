public interface ICompanionState
{
    string Name { get; }
    void Enter();
    void Tick();
    void Exit();
}