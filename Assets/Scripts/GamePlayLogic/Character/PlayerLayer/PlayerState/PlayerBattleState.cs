using UnityEngine;
using System;

public class PlayerBattleState : PlayerBaseState
{
    public enum BattlePhase
    {
        Ready, Wait, MoveComand, SkillComand, SkillTarget, Move, SkillCast,
        ReleaseMoveComand,
        ReleaseSkillComand, ReleaseSkillComandEnd, 
        End
    }

    private BattlePhase currentPhase;
    private Action skillChangedHandler;
    private GameNode confirmMoveNode;
    private GameNode targetNode;
    private SkillData currentSkill;

    private bool moveTargetConfirmed = false;
    private bool skillCastConfirmed = false;
    private bool endTurnConfirmed = false;
    private bool movedConfirmed = false;

    public PlayerBattleState(PlayerStateMachine stateMachine, PlayerCharacter character) : base(stateMachine, character)
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
                    ChangePhase(BattlePhase.MoveComand);
                }
                break;
            case BattlePhase.MoveComand:
                if (BattleManager.instance.IsSelectedNodeChange())
                {
                    GameNode selectedNode = BattleManager.instance.GetSelectedGameNode();
                    character.ShowDangerMovableAndTargetTilemap(selectedNode);
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                { 
                    BattleManager.instance.SetGridCursorAt(character.GetCharacterTransformToNode());
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    GameNode moveTargetNode = BattleManager.instance.GetSelectedGameNode();
                    if (!character.IsInMovableRange(moveTargetNode)) return;

                    confirmMoveNode = moveTargetNode;
                    if (confirmMoveNode != character.GetCharacterTransformToNode())
                    {
                        moveTargetConfirmed = true;
                    }
                    BattleManager.instance.GeneratePreviewCharacterInMovableRange(character);
                    ChangePhase(BattlePhase.SkillComand);
                }
                else if (Input.GetKeyDown(KeyCode.P))
                {
                    confirmMoveNode = character.GetCharacterTransformToNode();
                    endTurnConfirmed = true;
                    ChangePhase(BattlePhase.End);
                }
                break;
            case BattlePhase.SkillComand:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (moveTargetConfirmed)
                    {
                        moveTargetConfirmed = false;
                    }
                    ChangePhase(BattlePhase.MoveComand);
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    ChangePhase(BattlePhase.SkillTarget);
                }
                else if (Input.GetKeyDown(KeyCode.P))
                {
                    endTurnConfirmed = true;
                    if (moveTargetConfirmed)
                        ChangePhase(BattlePhase.Move);
                    else
                        ChangePhase(BattlePhase.ReleaseSkillComandEnd);
                }
                break;
            case BattlePhase.SkillTarget:
                if (BattleManager.instance.IsSelectedNodeChange())
                {
                    GameNode selectedNode = BattleManager.instance.GetSelectedGameNode();
                    targetNode = character.GetSkillTargetShowTilemap(confirmMoveNode, selectedNode);
                    
                    if (confirmMoveNode != null)
                    {
                        BattleManager.instance.ShowProjectileParabola(character, currentSkill, confirmMoveNode, selectedNode);
                    }
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (!movedConfirmed)
                        ChangePhase(BattlePhase.SkillComand);
                    else
                        ChangePhase(BattlePhase.ReleaseSkillComand);
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (BattleManager.instance.IsValidateSkillTarget(character, currentSkill, targetNode))
                    {
                        if (moveTargetConfirmed)
                            ChangePhase(BattlePhase.Move);
                        else
                            ChangePhase(BattlePhase.SkillCast);
                    }
                }
                break;
            case BattlePhase.ReleaseMoveComand:
                if (BattleManager.instance.IsSelectedNodeChange())
                {
                    GameNode selectedNode = BattleManager.instance.GetSelectedGameNode();
                    character.ShowDangerMovableAndTargetTilemap(selectedNode);
                }
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    GameNode moveTargetNode = BattleManager.instance.GetSelectedGameNode();
                    if (!character.IsInMovableRange(moveTargetNode)) return;

                    confirmMoveNode = moveTargetNode;
                    if (confirmMoveNode != character.GetCharacterTransformToNode())
                    {
                        moveTargetConfirmed = true;
                    }
                    BattleManager.instance.GeneratePreviewCharacterInMovableRange(character);
                    if (moveTargetConfirmed)
                        ChangePhase(BattlePhase.Move);
                    else
                        ChangePhase(BattlePhase.End);
                }
                break;
            case BattlePhase.Move:
                character.PathToTarget();
                if (!skillCastConfirmed && !endTurnConfirmed)
                {
                    if (character.pathRoute == null)
                    {
                        ChangePhase(BattlePhase.SkillCast);
                    }
                }
                else if (!skillCastConfirmed && moveTargetConfirmed && endTurnConfirmed)
                {
                    if (character.pathRoute == null)
                    {
                        ChangePhase(BattlePhase.ReleaseSkillComandEnd);
                    }
                }
                else if (skillCastConfirmed)
                {
                    if (character.pathRoute == null)
                    {
                        ChangePhase(BattlePhase.End);
                    }
                }
                break;
            case BattlePhase.ReleaseSkillComand:
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    ChangePhase(BattlePhase.SkillTarget);
                }
                else if (Input.GetKeyDown(KeyCode.P))
                {
                    endTurnConfirmed = true;
                    ChangePhase(BattlePhase.ReleaseSkillComandEnd);
                }
                break;
            case BattlePhase.SkillCast:
                if (moveTargetConfirmed && movedConfirmed)
                {
                    endTurnConfirmed = true;
                    skillCastConfirmed = true;
                    ChangePhase(BattlePhase.End);
                }
                else if (!moveTargetConfirmed && !movedConfirmed)
                {
                    skillCastConfirmed = true;
                    ChangePhase(BattlePhase.ReleaseMoveComand);
                }
                break;
            case BattlePhase.ReleaseSkillComandEnd:
                if (BattleManager.instance.IsOrientationChanged())
                {
                    Orientation orientation = BattleManager.instance.GetSelectedOrientation();
                    character.SetTransfromOrientation(orientation);
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    endTurnConfirmed = false;
                    BattleManager.instance.HideOrientationArrow();
                    ChangePhase(BattlePhase.ReleaseSkillComand);
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    BattleManager.instance.HideOrientationArrow();
                    BattleManager.instance.OnLoadNextTurn();
                    ChangePhase(BattlePhase.Wait);
                }
                break;
            case BattlePhase.End:
                if (BattleManager.instance.IsOrientationChanged())
                {
                    Orientation orientation = BattleManager.instance.GetSelectedOrientation();
                    character.SetTransfromOrientation(orientation);
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    BattleManager.instance.HideOrientationArrow();
                    BattleManager.instance.OnLoadNextTurn();
                    ChangePhase(BattlePhase.Wait);
                }
                break;
        }
    }

    public void ChangePhase(BattlePhase newPhase)
    {
        ExitPhase(currentPhase);
        currentPhase = newPhase;
        EnterPhase(newPhase);
        Debug.Log($"Enter: {currentPhase}");
    }

    public void ExitPhase(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.SkillComand:
                SkillUIManager.instance.onListOptionChanged -= skillChangedHandler;
                BattleManager.instance.CloseAllProjectileParabola();
                BattleUIManager.instance.CloseSkillUI();
                break;
            case BattlePhase.SkillTarget:
                BattleManager.instance.CloseAllProjectileParabola();
                break;
            case BattlePhase.ReleaseSkillComand:
                SkillUIManager.instance.onListOptionChanged -= skillChangedHandler;
                BattleManager.instance.CloseAllProjectileParabola();
                BattleUIManager.instance.CloseSkillUI();
                break;
        }
    }

    private void EnterPhase(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.Ready:
                break;
            case BattlePhase.Wait:
                confirmMoveNode = null;
                targetNode = null;
                currentSkill = null;
                moveTargetConfirmed = false;
                skillCastConfirmed = false;
                movedConfirmed = false;
                endTurnConfirmed = false;
                break;
            case BattlePhase.MoveComand:
                character.ShowDangerAndMovableTileFromNode();
                BattleManager.instance.SetGridCursorAt(character.GetCharacterTransformToNode());

                //  If get return to move command phase
                BattleManager.instance.DestroyPreviewModel();
                BattleUIManager.instance.ActivateActionPanel(true);
                BattleUIManager.instance.CloseSkillUI();
                BattleUIManager.instance.ActiveAllCharacterInfoTip(false);
                break;
            case BattlePhase.SkillComand:
                SkillComandInstruction();
                break;
            case BattlePhase.SkillTarget:
                BattleManager.instance.ActivateMoveCursorAndHide(true, false);
                BattleManager.instance.ShowOppositeTeamParabola(character, confirmMoveNode);
                BattleUIManager.instance.ActiveAllCharacterInfoTip(true);
                break;
            case BattlePhase.ReleaseMoveComand:
                character.ShowDangerAndMovableTileFromNode();
                BattleManager.instance.SetGridCursorAt(character.GetCharacterTransformToNode());
                BattleUIManager.instance.ActiveAllCharacterInfoTip(true);
                break;
            case BattlePhase.Move:
                BattleManager.instance.ActivateMoveCursorAndHide(false, true);
                BattleUIManager.instance.CloseSkillUI();
                BattleUIManager.instance.ActiveAllCharacterInfoTip(false);
                CameraMovement.instance.ChangeFollowTarget(character.transform);
                character.SetPathRoute(confirmMoveNode);
                character.ShowDangerMovableAndTargetTilemap(confirmMoveNode);
                movedConfirmed = true;
                break;
            case BattlePhase.ReleaseSkillComand:
                SkillComandInstruction();
                break;
            case BattlePhase.SkillCast:
                BattleManager.instance.DestroyPreviewModel();
                BattleManager.instance.CastSkill(character, currentSkill, confirmMoveNode, targetNode);
                break;
            case BattlePhase.ReleaseSkillComandEnd:
                EndInstruction();
                break;
            case BattlePhase.End:
                EndInstruction();
                break;
        }
    }

    private void SkillComandInstruction()
    {
        BattleManager.instance.SetGridCursorAt(confirmMoveNode);
        BattleManager.instance.ActivateMoveCursorAndHide(false, false);
        BattleManager.instance.ShowOppositeTeamParabola(character, confirmMoveNode);
        BattleUIManager.instance.ActivateActionPanel(false);
        BattleUIManager.instance.OpenUpdateSkillUI(character);
        BattleUIManager.instance.ActiveAllCharacterInfoTip(true);

        skillChangedHandler = () =>
        {
            currentSkill = SkillUIManager.instance.GetCurrentSelectedSkill();
            character.SetSkill(currentSkill);
            character.ShowSkillTilemap(confirmMoveNode);
        };
        SkillUIManager.instance.onListOptionChanged += skillChangedHandler;
        //  First designate skill
        currentSkill = SkillUIManager.instance.GetCurrentSelectedSkill();

        character.ShowSkillTilemap(confirmMoveNode);
    }
    private void EndInstruction()
    {
        character.ResetVisualTilemap();
        BattleManager.instance.ActivateMoveCursorAndHide(false, true);
        BattleManager.instance.SetupOrientationArrow(character, confirmMoveNode);
        BattleManager.instance.DestroyPreviewModel();
        BattleUIManager.instance.ActiveAllCharacterInfoTip(false);
        BattleUIManager.instance.ActivateActionPanel(false);
    }
}
