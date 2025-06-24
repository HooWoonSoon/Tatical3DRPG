using UnityEngine;

public class IdleStateExplore : CharacterBaseState
{
    public IdleStateExplore(StateMachine stateMachine, Character character) : base(stateMachine, character)
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
        if (character.xInput != 0 || character.zInput != 0)
        {
            stateMachine.ChangeSubState(character.moveStateExplore);
        }
    }
}