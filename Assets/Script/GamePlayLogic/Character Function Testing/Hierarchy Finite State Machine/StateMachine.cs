public class StateMachine
{
    public CharacterBaseState roofState { get; private set; }
    public CharacterBaseState subState { get; private set; }
   
    public void Initialize(CharacterBaseState roofState, CharacterBaseState subState)
    {
        this.roofState = roofState;
        this.subState = subState;
        roofState.Enter();
        subState.Enter();
    }

    public void ChangeState(CharacterBaseState newState)
    {
        if (roofState == newState) { return; }
        roofState.Exit();
        roofState = newState;
        roofState.Enter();
    }

    public void ChangeSubState(CharacterBaseState newState)
    {
        if (subState == newState) { return; }
        subState.Exit();
        subState = newState;
        subState.Enter();
    }
}