public class StateMachine
{
    public PlayerBaseState currentState { get; private set; }
   
    public void Initialize(PlayerBaseState currentState)
    {
        this.currentState = currentState;
        currentState.Enter();
    }

    public void ChangeState(PlayerBaseState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
}