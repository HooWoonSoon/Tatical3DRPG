using UnityEngine;

public class IdleStateBattle : PlayerBaseState
{
    public IdleStateBattle(StateMachine stateMachine, PlayerStateMachine player) : base(stateMachine, player)
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
