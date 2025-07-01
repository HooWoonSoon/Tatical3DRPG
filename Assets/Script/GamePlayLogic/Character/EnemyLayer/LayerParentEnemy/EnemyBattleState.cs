using UnityEngine;

public class EnemyBattleState : EnemyBaseState
{
    public EnemyBattleState(EnemyStateMachine stateMachine, EnemyCharacter character) : base(stateMachine, character)
    {
    }

    public override void Enter()
    {
        base.Enter();
        character.stateMechine.ChangeSubState(character.movePathStateBattle);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (stateMachine.roofState == this && character.IsYourTurn(character))
        {
            character.ResetVisualTilemap();
            character.ShowVisualTilemapMahattasRange();
            if (Input.GetKeyDown(KeyCode.Return))
            {
                CTTimeline.instance.NextNumber();
            }
        }
    }
}

