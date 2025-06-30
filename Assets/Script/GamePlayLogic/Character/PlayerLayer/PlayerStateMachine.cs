public class PlayerStateMachine
{
    public PlayerBaseState roofState { get; private set; }
    public PlayerBaseState subState { get; private set; }
   
    public void Initialize(PlayerBaseState roofState, PlayerBaseState subState)
    {
        this.roofState = roofState;
        this.subState = subState;
        this.roofState.Enter();
        this.subState.Enter();
    }

    public void ChangeRoofState(PlayerBaseState newState)
    {
        if (roofState == newState || roofState == null) { return; }
        roofState.Exit();
        roofState = newState;
        roofState.Enter();
    }

    public void ChangeSubState(PlayerBaseState newState)
    {
        if (subState == newState || subState == null) { return; }
        subState.Exit();
        subState = newState;
        subState.Enter();
    }
}