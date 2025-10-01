
using UnityEngine;

public class EnemyMovePathStateBattle : EnemyBaseState
{
    public EnemyMovePathStateBattle(EnemyStateMachine stateMachine, EnemyCharacter character) : base(stateMachine, character)
    {
    }

    public override void Enter()
    {
        base.Enter();
        CameraMovement.instance.ChangeFollowTarget(character.transform);
    }

    public override void Exit()
    {
        base.Exit();
        character.ResetVisualTilemap();
    }

    public override void Update()
    {
        base.Update();
        character.PathToTarget();
        if (character.pathRoute == null)
        {
            if (character.currentSkill != null)
            {
                stateMachine.ChangeSubState(character.skillCastStateBattle);
            }
            else
            {
                stateMachine.ChangeSubState(character.idleStateBattle);
            }
        }
    }
}

