
public class EnemyStateMachine
{
    public EnemyBaseState roofState { get; private set; }
    public EnemyBaseState subState { get; private set; }

    public void Initialize(EnemyBaseState roofState, EnemyBaseState subState)
    {
        this.roofState = roofState;
        this.subState = subState;
        this.roofState.Enter();
        this.subState.Enter();
    }

    public void ChangeRoofState(EnemyBaseState newState)
    {
        if (roofState == newState || roofState == null) { return; }
        roofState.Exit();
        roofState = newState;
        roofState.Enter();
    }

    public void ChangeSubState(EnemyBaseState newState)
    {
        if (subState == newState || subState == null) { return; }
        subState.Exit();
        subState = newState;
        subState.Enter();
    }
}

