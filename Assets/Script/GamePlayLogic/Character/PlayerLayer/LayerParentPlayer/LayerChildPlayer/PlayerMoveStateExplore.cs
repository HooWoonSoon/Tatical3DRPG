
using UnityEngine;

public class PlayerMoveStateExplore : PlayerBaseState
{
    public PlayerMoveStateExplore(PlayerStateMachine stateMachine, PlayerCharacter character) : base(stateMachine, character)
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

        character.Move(character.xInput, character.zInput);
        character.UpDownHill(character.xInput, character.zInput);

        if (character.xInput == 0 && character.zInput == 0)
        {
            stateMachine.ChangeSubState(character.idleStateExplore);
        }
    }
}

