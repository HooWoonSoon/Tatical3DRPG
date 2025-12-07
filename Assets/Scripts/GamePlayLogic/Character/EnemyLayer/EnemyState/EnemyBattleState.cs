using System.Collections.Generic;
using UnityEngine;
using Tactics.AI;

public enum AgentBattlePhase
{
    Ready, Wait, Thinking, Move, SkillCast,
    ReleaseMoveThinking,
    ReleaseSkillThinking,
    End
}
public class EnemyBattleState : EnemyBaseState
{
    private AgentBattlePhase currentPhase;
    private float phaseStartTime;
    private GameNode confrimMoveNode;

    private GameNode targetNode;
    private SkillData currentSkill;

    private bool movedConfirmed = false;
    private bool skillCastConfirmed = false;

    public EnemyBattleState(EnemyStateMachine stateMachine, EnemyCharacter character) : base(stateMachine, character)
    {
    }

    public override void Enter()
    {
        base.Enter();
        character.ResetVisualTilemap();
        currentPhase = AgentBattlePhase.Ready;
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
            case AgentBattlePhase.Ready:
                character.PathToTarget();
                if (character.pathRoute == null)
                {
                    ChangePhase(AgentBattlePhase.Wait);
                }
                break;
            case AgentBattlePhase.Wait:
                if (character.IsYourTurn(character))
                {
                    ChangePhase(AgentBattlePhase.Thinking);
                }
                break;
            case AgentBattlePhase.Thinking:
                if (phaseStartTime > 0.5f)
                {
                    if (character.pathRoute != null)
                        ChangePhase(AgentBattlePhase.Move);
                    else if (character.currentSkill != null)
                        ChangePhase(AgentBattlePhase.SkillCast);
                    else
                        ChangePhase(AgentBattlePhase.End);
                }
                break;
            case AgentBattlePhase.ReleaseMoveThinking:
                if (phaseStartTime > 0.5f)
                {
                    if (character.pathRoute != null)
                        ChangePhase(AgentBattlePhase.Move);
                    else
                        ChangePhase(AgentBattlePhase.End);
                }
                break;
            case AgentBattlePhase.Move:
                character.PathToTarget();
                if (character.pathRoute == null)
                {
                    movedConfirmed = true;
                    if (!skillCastConfirmed && currentSkill != null)
                        ChangePhase(AgentBattlePhase.SkillCast);
                    else if (!skillCastConfirmed && currentSkill == null)
                        ChangePhase(AgentBattlePhase.ReleaseSkillThinking);
                    else
                        ChangePhase(AgentBattlePhase.End);
                }
                break;
            case AgentBattlePhase.ReleaseSkillThinking:
                if (phaseStartTime > 0.5)
                {
                    if (currentSkill != null)
                        ChangePhase(AgentBattlePhase.SkillCast);
                    else
                        ChangePhase(AgentBattlePhase.End);
                }
                break;
            case AgentBattlePhase.SkillCast:
                if (phaseStartTime > currentSkill.skillCastTime)
                {
                    skillCastConfirmed = true;
                    if (!movedConfirmed)
                        ChangePhase(AgentBattlePhase.ReleaseMoveThinking);
                    else
                        ChangePhase(AgentBattlePhase.End);
                }
                break;
            case AgentBattlePhase.End:
                if (phaseStartTime > 0.5f)
                {
                    BattleManager.instance.OnLoadNextTurn();
                    ChangePhase(AgentBattlePhase.Wait);
                }
                break;
        }
    }

    public void ChangePhase(AgentBattlePhase newPhase)
    {
        currentPhase = newPhase;
        phaseStartTime = 0;
        EnterPhase(newPhase);
        if (character.debugMode)
            Debug.Log($"{character} enter to {newPhase}");
    }

    public void EnterPhase(AgentBattlePhase phase)
    {
        switch (phase)
        {
            case AgentBattlePhase.Ready:
                break;
            case AgentBattlePhase.Wait:
                confrimMoveNode = null;
                currentSkill = null;
                targetNode = null;
                movedConfirmed = false;
                skillCastConfirmed = false;
                break;
            case AgentBattlePhase.Thinking:
                CameraController.instance.ChangeFollowTarget(character.transform);
                float startTime = Time.realtimeSinceStartup;
                character.decisionSystem.MakeDecision();
                character.decisionSystem.GetResult(out currentSkill, out confrimMoveNode, out targetNode);
                //Debug.Log($"Decision Time: {Time.realtimeSinceStartup - startTime}");

                if (confrimMoveNode != null)
                {
                    character.SetPathRoute(confrimMoveNode);
                }
                character.SetSkillAndTarget(currentSkill, targetNode);
                break;
            case AgentBattlePhase.ReleaseMoveThinking:
                character.decisionSystem.MakeDecision(true, false);
                character.decisionSystem.GetResult(out currentSkill, out confrimMoveNode, out targetNode);

                if (confrimMoveNode != null)
                {
                    character.SetPathRoute(confrimMoveNode);
                }
                break;
            case AgentBattlePhase.Move:
                if (confrimMoveNode != null)
                {
                    character.ShowDangerMovableAndTargetTilemap(confrimMoveNode);
                    CameraController.instance.ChangeFollowTarget(character.transform);
                }
                break;
            case AgentBattlePhase.SkillCast:
                if (currentSkill != null)
                {
                    character.ShowSkillTargetTilemap();
                    BattleManager.instance.CastSkill(character, currentSkill, confrimMoveNode, targetNode);
                }
                break;
            case AgentBattlePhase.End:
                character.ResetVisualTilemap();
                break;
        }
    }
}

