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
        if (movaToNode != null)
        {
            character.SetPathRoute(movaToNode);
            character.ShowDangerMovableAndTargetTilemap(movaToNode);
        }
        character.SetSkillAndTarget(skill, skillTargetNode);
        character.ShowDangerMovableAndTargetTilemap(character.GetCharacterOriginNode());
        CameraMovement.instance.ChangeFollowTarget(character.transform);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Return))
        {
            BattleManager.instance.OnLoadNextTurn();
        }
        if (!character.IsYourTurn(character))
        {
            stateMachine.ChangeRoofState(character.waitState);
        }
        if (character.pathRoute != null)
        {
            stateMachine.ChangeSubState(character.movePathStateBattle);
        }
        else
        {
            if (character.currentSkill != null)
            {
                stateMachine.ChangeSubState(character.skillCastStateBattle);
            }
            else
            {
                if (timeInState >= 2.0f)
                {
                    stateMachine.ChangeSubState(character.idleStateBattle);
                }
            }
        }
    }
}

