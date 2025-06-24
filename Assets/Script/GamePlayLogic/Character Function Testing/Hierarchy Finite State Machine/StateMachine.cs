public class StateMachine
{
    public CharacterBaseState currentState { get; private set; }
   
    public void Initialize(CharacterBaseState currentState)
    {
        this.currentState = currentState;
        currentState.Enter();
    }

    public void ChangeState(CharacterBaseState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
}