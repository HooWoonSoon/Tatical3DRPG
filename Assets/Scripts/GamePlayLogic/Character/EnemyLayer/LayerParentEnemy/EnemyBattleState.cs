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
        character.ShowDangerAndMovableTileFromNode();
        DecisionMaker decisionMaker = new DecisionMaker(character);
        decisionMaker.MakeDecision();
        decisionMaker.GetResult(out SkillData skill, out GameNode targetNode);
        Debug.Log($"Target Node: {targetNode}");
        Debug.Log($"Skill: {skill}");
        if (targetNode != null && skill != null)
        {
            Debug.Log($"Move To {targetNode.GetVector()}, Use {skill.skillName}");
        }
        else
        {
            Debug.Log("No valid action");
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

