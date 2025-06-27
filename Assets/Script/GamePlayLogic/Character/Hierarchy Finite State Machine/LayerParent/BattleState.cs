using UnityEngine;

public class BattleState : CharacterBaseState
{
    public BattleState(StateMachine stateMachine, CharacterBase character) : base(stateMachine, character)
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
