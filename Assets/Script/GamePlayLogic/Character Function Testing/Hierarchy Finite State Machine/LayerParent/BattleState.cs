using UnityEngine;

public class BattleState : CharacterBaseState
{
    public IdleStateBattle idleStateBattle;
    public AttackStateBattle attackStateBattle;

    public BattleState(StateMachine stateMachine, Character player) : base(stateMachine, player)
    {
        attackStateBattle = new AttackStateBattle(stateMachine, player);
        idleStateBattle = new IdleStateBattle(stateMachine, player);
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Battle State");
        SetSubState(idleStateBattle);
    }

    public override void Exit()
    {
        base.Exit();
        currentSubState.Exit();
    }

    public override void SetSubState(CharacterBaseState newSubState)
    {
        base.SetSubState(newSubState);
    }
}
