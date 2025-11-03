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
    private GameNode confrimMoveNode;

    private GameNode targetNode;
    private SkillData currentSkill;

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
        Debug.Log($"{character} enter to {newPhase}");
    }

    public void EnterPhase(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.Ready:
                break;
            case BattlePhase.Wait:
                confrimMoveNode = null;
                currentSkill = null;
                targetNode = null;
                break;
            case BattlePhase.Thinking:
                CameraMovement.instance.ChangeFollowTarget(character.transform);
                float startTime = Time.realtimeSinceStartup;
                DecisionSystem decisionMaker = new DecisionSystem(character);
                decisionMaker.GetResult(out currentSkill, out confrimMoveNode, out targetNode);
                //Debug.Log($"Decision Time: {Time.realtimeSinceStartup - startTime}");

                if (confrimMoveNode != null)
                {
                    character.SetPathRoute(confrimMoveNode);
                }
                character.SetSkillAndTarget(currentSkill, targetNode);
                break;
            case BattlePhase.Move:
                character.ShowDangerMovableAndTargetTilemap(confrimMoveNode);
                CameraMovement.instance.ChangeFollowTarget(character.transform);
                break;
            case BattlePhase.SkillCast:
                character.ShowSkillTargetTilemap();
                BattleManager.instance.CastSkill(character, currentSkill, confrimMoveNode, targetNode);
                break;
            case BattlePhase.End:
                character.ResetVisualTilemap();
                break;
        }
    }
}

