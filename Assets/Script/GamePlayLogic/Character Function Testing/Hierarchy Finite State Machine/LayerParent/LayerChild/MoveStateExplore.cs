
public class MoveStateExplore : CharacterBaseState
{
    public MoveStateExplore(StateMachine stateMachine, Character player) : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (player.xInput == 0 && player.zInput == 0)
        {
            SetSubState(player.explorationState.idleStateExplore);
        }
    }
}

