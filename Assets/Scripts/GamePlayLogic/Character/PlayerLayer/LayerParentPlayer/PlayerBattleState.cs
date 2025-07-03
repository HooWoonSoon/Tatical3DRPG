using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleState : PlayerBaseState
{
    public PlayerBattleState(PlayerStateMachine stateMachine, PlayerCharacter character) : base(stateMachine, character)
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
        if (!character.IsYourTurn(character)) { return; }
        stateMachine.ChangeRoofState(character.waitState);
    }
}
