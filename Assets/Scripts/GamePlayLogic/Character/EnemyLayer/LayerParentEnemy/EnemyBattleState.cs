using UnityEngine;

public class EnemyBattleState : EnemyBaseState
{
    public EnemyBattleState(EnemyStateMachine stateMachine, EnemyCharacter character) : base(stateMachine, character)
    {
    }

    public override void Enter()
    {
        base.Enter();
        character.ResetVisualTilemap();
        character.ShowVisualTilemapMahattasRange(character.data.movableRange);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (!character.IsYourTurn(character))
        {
            stateMachine.ChangeRoofState(character.waitState);
        }
    }
}

