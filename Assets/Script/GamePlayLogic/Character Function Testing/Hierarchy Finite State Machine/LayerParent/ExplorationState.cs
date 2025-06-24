using UnityEngine;

public class ExplorationState : CharacterBaseState
{
    public IdleStateExplore idleStateExplore;
    public MoveStateExplore moveStateExplore;

    public ExplorationState(StateMachine stateMachine, Character player) : base(stateMachine, player)
    {
        idleStateExplore = new IdleStateExplore(stateMachine, player);
        moveStateExplore = new MoveStateExplore(stateMachine, player);
    }

    public override void Enter()
    {
        base.Enter();
        SetSubState(idleStateExplore);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void SetSubState(CharacterBaseState newSubState)
    {
        base.SetSubState(newSubState);
    }
}
