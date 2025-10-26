using UnityEngine;

public class PlayerIdleStateExplore : PlayerBaseState
{
    public PlayerIdleStateExplore(PlayerStateMachine stateMachine, PlayerCharacter character) : base(stateMachine, character)
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

        character.CalculateVelocity();
        character.YCoordinateAllignment();

        if (character.xInput != 0 || character.zInput != 0)
        {
            stateMachine.ChangeState(character.moveStateExplore);
        }
    }
}