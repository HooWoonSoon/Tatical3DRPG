using System.Collections.Generic;
using UnityEngine;

public class EnemyBattleState : EnemyBaseState
{
    public enum BattlePhase
    {
        Ready, Wait, Thinking, Move, SkillCast,
        End
    }

    private BattlePhase currentPhase;
    private float phaseStartTime;

    public EnemyBattleState(EnemyStateMachine stateMachine, EnemyCharacter character) : base(stateMachine, character)
    {
    }

    public override void Enter()
    {
        base.Enter();
        character.ResetVisualTilemap();
        currentPhase = BattlePhase.Ready;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        phaseStartTime += Time.deltaTime;

        switch (currentPhase)
        {
            case BattlePhase.Ready:
                character.PathToTarget();
                if (character.pathRoute == null)
                {
                    ChangePhase(BattlePhase.Wait);
                }
                break;
            case BattlePhase.Wait:
                if (character.IsYourTurn(character))
                {
                    ChangePhase(BattlePhase.Thinking);
                }
                break;
            case BattlePhase.Thinking:
                if (phaseStartTime > 0.5f)
                {
                    ChangePhase(BattlePhase.Move);
                }
                break;
            case BattlePhase.Move:
                character.PathToTarget();
                if (character.pathRoute == null)
                {
                    if (character.currentSkill != null)
                        ChangePhase(BattlePhase.SkillCast);
                    else
                        ChangePhase(BattlePhase.End);                
                }
                break;
            case BattlePhase.SkillCast:
                if (phaseStartTime > character.currentSkill.skillCastTime)
                {
                    character.SkillCalculate();
                    ChangePhase(BattlePhase.End);
                }
                break;
            case BattlePhase.End:
                if (phaseStartTime > 0.5f)
                {
                    BattleManager.instance.OnLoadNextTurn();
                    ChangePhase(BattlePhase.Wait);
                }
                break;
        }
    }

    public void ChangePhase(BattlePhase newPhase)
    {
        currentPhase = newPhase;
        phaseStartTime = 0;
        EnterPhase(newPhase);
    }

    public void EnterPhase(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.Ready:
                break;
            case BattlePhase.Wait:
                break;
            case BattlePhase.Thinking:
                CameraMovement.instance.ChangeFollowTarget(character.transform);
                float startTime = Time.realtimeSinceStartup;
                DecisionMaker decisionMaker = new DecisionMaker(character);
                decisionMaker.GetResult(out SkillData skill, out GameNode movaToNode, out GameNode skillTargetNode);
                Debug.Log($"Decision Time: {Time.realtimeSinceStartup - startTime}");
                if (movaToNode != null)
                {
                    character.SetPathRoute(movaToNode);
                    character.ShowDangerMovableAndTargetTilemap(movaToNode);
                }
                character.SetSkillAndTarget(skill, skillTargetNode);
                break;
            case BattlePhase.Move:
                CameraMovement.instance.ChangeFollowTarget(character.transform);
                break;
            case BattlePhase.SkillCast:
                character.ShowSkillTargetTilemap();
                break;
            case BattlePhase.End:
                character.ResetVisualTilemap();
                break;
        }
    }
}

