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
        character.ResetVisualTilemap();
        DecisionMaker decisionMaker = new DecisionMaker(character);
        decisionMaker.GetResult(out SkillData skill, out GameNode movaToNode, out GameNode skillTargetNode);
        character.SetPathRoute(movaToNode);
        character.ShowDangerMovableAndTargetTilemap(movaToNode);
        character.SetSkillAndTarget(skill, skillTargetNode);
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
        if (character.pathRoute != null)
        {
            stateMachine.ChangeSubState(character.movePathStateBattle);
        }
    }
}

