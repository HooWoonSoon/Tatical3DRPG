
using UnityEngine;

public class EnemyMovePathStateBattle : EnemyBaseState
{
    public EnemyMovePathStateBattle(EnemyStateMachine stateMachine, EnemyCharacter character) : base(stateMachine, character)
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
        character.PathfindingMoveToTarget();
    }
}

