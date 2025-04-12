public class ExplorationState : PlayerBaseState
{
    public IdleStateExplore idleStateExplore;

    public ExplorationState(StateMachine stateMachine, PlayerStateMachine player) : base(stateMachine, player)
    {
    }
}
