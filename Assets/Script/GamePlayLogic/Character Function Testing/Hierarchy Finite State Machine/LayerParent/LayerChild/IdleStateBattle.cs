using UnityEngine;

public class IdleStateBattle : CharacterBaseState
{
    public IdleStateBattle(StateMachine stateMachine, Character player) : base(stateMachine, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter Idle State Battle");
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit Idle State Battle");
    }

    public override void Update()
    {
        base.Update();
    }
}
