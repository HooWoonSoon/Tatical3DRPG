using UnityEngine;

public class BattleState : CharacterBaseState
{
    public IdleStateBattle idleStateBattle;
    public AttackStateBattle attackStateBattle;

    public BattleState(StateMachine stateMachine, Character character) : base(stateMachine, character)
    {

    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Battle State");
    }

    public override void Exit()
    {
        base.Exit();
    }
}
