using System.Collections.Generic;
using UnityEngine;

public class EnemyBattleState : EnemyBaseState
{
    public EnemyBattleState(EnemyStateMachine stateMachine, EnemyCharacter character) : base(stateMachine, character)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Execute");
        character.ResetVisualTilemap();
        character.ShowDangerAndMovableTileFromNode();
        DecisionMaker decisionMaker = new DecisionMaker(character);
        decisionMaker.MakeDecision();
        decisionMaker.GetResult(out SkillData skill, out GameNode targetNode);
        if (targetNode != null)
        {
            character.pathRoute = character.GetPathRoute(targetNode);
            character.pathRoute.DebugPathRoute();
            stateMachine.ChangeSubState(character.movePathStateBattle);
        }
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit Battle State");
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

