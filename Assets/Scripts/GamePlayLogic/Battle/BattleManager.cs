using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : Entity
{
    public List<TeamDeployment> battleTeams = new List<TeamDeployment>();
    public List<CharacterBase> joinedBattleUnits = new List<CharacterBase>();
    public bool isBattleStarted = false;

    [Header("Cursor")]
    public GridCursor gridCursor;
    private GameNode lastSelectedNode;

    [Header("Orientation")]
    public BattleOrientationArrow orientationArrow;
    private Orientation lastOrientation;

    [Header("Preview")]
    public Material previewMaterial;
    private GameObject previewCharacter;

    public event Action onStartBattle;
    public event Action onLoadNextTurn;
    public event Action onConfrimCallback;
    public event Action onCancelCallback;
    public static BattleManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }
    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnEnter();
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            OnCancel();
        }
    }

    #region Event Callback
    private void OnEnter()
    {
        onConfrimCallback?.Invoke();
        onConfrimCallback = null;
    }
    private void OnCancel()
    {
        onConfrimCallback = null;
        onCancelCallback?.Invoke();
        onCancelCallback = null;
    }
    #endregion

    #region Setup Battle
    public void SetJoinedBattleUnit(HashSet<CharacterBase> joinedBattleUnits)
    {
        this.joinedBattleUnits = joinedBattleUnits.ToList();
    }
    public void SetJoinedBattleUnit(List<CharacterBase> joinedBattleUnits)
    {
        this.joinedBattleUnits = joinedBattleUnits;
    }
    #endregion
    
    #region Battle Unit Refine Path
    private void EnterBattleUnitRefinePath()
    {
        List<PathRoute> pathRoutes = GetBattleUnitRefinePath();

        for (int i = 0; i < joinedBattleUnits.Count; i++)
        {
            joinedBattleUnits[i].ReadyBattle();
        }
        for (int i = 0; i < pathRoutes.Count; i++)
        {
            pathRoutes[i].character.SetPathRoute(pathRoutes[i]);
            Debug.Log($"{pathRoutes[i].character} to target {pathRoutes[i].targetPosition}");
        }
    }
    private List<PathRoute> GetBattleUnitRefinePath()
    {
        List<PathRoute> pathRoutes = new List<PathRoute>();
        HashSet<Vector3Int> occupiedPos = new HashSet<Vector3Int>();
        foreach (CharacterBase character in joinedBattleUnits)
        {
            int iteration = 1;
            bool found = false;
            Vector3Int unitPosition = character.GetCharacterNodePos();
            while (iteration <= 16 && !found)
            {
                List<Vector3Int> optionPos = character.GetUnlimitedMovablePos(iteration, occupiedPos);
                List<Vector3Int> sortPos = Utils.SortTargetRangeByDistance(unitPosition, optionPos);

                for (int i = 0; i < sortPos.Count; i++)
                {
                    Vector3Int target = sortPos[i];
                    if (occupiedPos.Contains(target)) { continue; }

                    PathRoute route = pathFinding.GetPathRoute(unitPosition, target, 1, 1);
                    if (route == null || route.pathRouteList == null || route.pathRouteList.Count == 0)
                        continue;

                    pathRoutes.Add(new PathRoute
                    {
                        character = character,
                        targetPosition = sortPos[i],
                        pathRouteList = new List<Vector3>(route.pathRouteList),
                        pathIndex = 0
                    });
                    occupiedPos.Add(sortPos[i]);
                    found = true;
                    //Debug.Log($"Adding PathRoute for {character.name} to {sortPos[i]}, route count {route.pathRouteList.Count}");

                    break;
                }
                iteration++;
            }

            if (!found)
            {
                Debug.LogError($"{character.name} Not Found Path, Origin Position {unitPosition}");
                return null;
            }
        }
        return pathRoutes;
    }
    #endregion
    
    #region Direct Battle
    public void PreapreBattleContent()
    {
        if (isBattleStarted) { return; }
        FindJoinedTeam();
        EnterBattleUnitRefinePath();
        BattleUIManager.instance.PrepareBattleUI();
        CTTimeline.instance.SetJoinedBattleUnit(joinedBattleUnits);
        CTTimeline.instance.SetupTimeline();
        BattleUIManager.instance.OnBattleUIFinish += () =>
        {
            Debug.Log("StartBattle");
            onStartBattle?.Invoke();
            isBattleStarted = true;
        };
    }
    private void FindJoinedTeam()
    {
        foreach (CharacterBase character in joinedBattleUnits)
        {
            TeamDeployment team = character.currentTeam;
            if (!battleTeams.Contains(team))
            {
                battleTeams.Add(team);
            }
        }
    }
    #endregion
    
    #region Battle Request
    public void RequestBattle(List<CharacterBase> allBattleCharacter, 
        Action confirmAction = null, Action cancelAction = null)
    {
        onConfrimCallback = () =>
        {
            SetJoinedBattleUnit(allBattleCharacter);
            PreapreBattleContent();
            confirmAction?.Invoke();
        };

        onCancelCallback = () =>
        {
            cancelAction?.Invoke();
        };
    }
    public void ClearEventCallback(Action action = null)
    {
        onConfrimCallback = null;
        onCancelCallback = null;
        action?.Invoke();
    }
    #endregion

    public void EndBattle()
    {
        isBattleStarted = false;
        battleTeams.Clear();
        joinedBattleUnits.Clear();
    }

    #region Cursor Gizmos
    public void ActivateMoveCursorAndHide(bool active, bool hide)
    {
        gridCursor.ActivateMoveCursor(active, hide);
    }
    public void SetGridCursorAt(GameNode target)
    {
        gridCursor.HandleGridCursor(target);
    }
    public GameNode GetSelectedGameNode()
    {
        return gridCursor.currentNode;
    }
    public bool IsSelectedNodeChange()
    {
        if (gridCursor.currentNode != lastSelectedNode)
        {
            lastSelectedNode = gridCursor.currentNode;
            return true;
        }
        return false;
    }
    #endregion

    #region Skill Target
    public bool IsValidateSkillTarget(CharacterBase character,
        SkillData currentSkill, GameNode targetNode)
    {
        if (targetNode == null) { return false; }

        CharacterBase targetCharacter = targetNode.GetUnitGridCharacter();
        if (targetCharacter == null) { return false; }

        TeamDeployment team = character.currentTeam;
        bool isAlly = team.teamCharacter.Contains(targetCharacter);

        switch (currentSkill.targetType)
        {
            case SkillTargetType.Self:
                return targetCharacter == character;

            case SkillTargetType.Both:
                return true;

            case SkillTargetType.Opposite:
                if (isAlly)
                {
                    Debug.Log("Invalid Target - Same team member");
                    return false;
                }
                return true;

            case SkillTargetType.Our:
                if (!isAlly)
                {
                    Debug.Log("Invalid Target - Non team member");
                    return false;
                }
                return true;

            default:
                Debug.LogWarning("Not define target");
                return false;
        }
    }
    #endregion

    #region Preview Parabola
    public void ShowProjectileParabola(CharacterBase selfCharacter,
        GameNode originNode, GameNode targetNode,
        bool checkInRange = false, bool forceOffset = false)
    {
        if (targetNode == null)
        {
            Debug.Log("No obtained node");
            return;
        }

        ParabolaRenderer parabola = selfCharacter.GetComponentInChildren<ParabolaRenderer>();
        if (parabola == null)
        {
            Debug.LogWarning("Missing Parabola Component in character");
            return;
        }

        if (originNode == null)
            originNode = selfCharacter.GetCharacterOriginNode();

        Vector3 offset = new Vector3(0, 1f, 0);
        Vector3 originPos = originNode.GetVector();
        Vector3 targetPos = targetNode.GetVector();

        CharacterBase targetCharacter = targetNode.GetUnitGridCharacter();
        if (targetCharacter != null)
            targetPos = targetCharacter.transform.position + offset;
        else if (forceOffset)
            targetPos += offset;

        if (originPos == targetPos)
        {
            Debug.Log("Invalid parabola, target to self");
            CloseProjectileParabola(selfCharacter);
            return;
        }

        foreach (SkillData skill in selfCharacter.skillData)
        {
            if (!skill.isProjectile)
                continue;

            if (checkInRange)
            {
                List<GameNode> influenceNodes = skill.GetInflueneNode(world, originNode);
                if (!influenceNodes.Contains(targetNode))
                    continue;
            }

            parabola.DrawProjectileVisual(originPos + offset, targetPos, skill.initialElevationAngle);
            return;
        }
    }

    public void CloseAllProjectileParabola()
    {
        for (int i = 0; i < joinedBattleUnits.Count; i++)
            CloseProjectileParabola(joinedBattleUnits[i]);
    }
    public void CloseProjectileParabola(CharacterBase character)
    {
        LineRenderer lineRenderer = character.GetComponentInChildren<LineRenderer>();
        if (lineRenderer == null) { return; }
        lineRenderer.positionCount = 0;
    }
    public void ShowOppositeTeamParabola(CharacterBase targetCharacter, GameNode targetNode)
    {
        List<CharacterBase> oppositeUnit = GetCharacterOpposites(targetCharacter);
        for (int i = 0; i < oppositeUnit.Count; i++)
        {
            GameNode originNode = oppositeUnit[i].GetCharacterOriginNode();
            if (targetNode == null)
                targetNode = targetCharacter.GetCharacterOriginNode();
            ShowProjectileParabola(oppositeUnit[i], originNode, targetNode, true, true);
        }
    }
    #endregion

    public void CastSkill(CharacterBase selfCharacter, SkillData currentSkill, GameNode originNode, GameNode targetNode)
    {
        if (currentSkill.isProjectile)
        {
            GameObject projectilePrefab = Instantiate(currentSkill.projectTilePrefab, originNode.GetVector(), Quaternion.identity);
            Projectile projectile = projectilePrefab.GetComponent<Projectile>();

            Vector3 offset = new Vector3(0, 2f, 0);
            Vector3 targetPos = targetNode.GetVector();
            CharacterBase targetCharacter = targetNode.GetUnitGridCharacter();
            if (targetCharacter != null)
                targetPos = targetCharacter.transform.position + offset;

            if (projectile != null)
            {
                if (originNode == null)
                    projectile.LaunchToTarget(selfCharacter, currentSkill, 
                        selfCharacter.transform.position + offset, targetPos);
                else
                    projectile.LaunchToTarget(selfCharacter, currentSkill,
                        originNode.GetVector() + offset, targetPos);
            }
        }
    }

    #region Preview Character
    public void GeneratePreviewCharacterInMovableRange(CharacterBase character)
    {
        List<GameNode> gameNodes = character.GetMovableNode();
        if (gameNodes.Contains(lastSelectedNode))
        {
            GeneratePreviewCharacter(character);
        }
    }
    public void GeneratePreviewCharacter(CharacterBase character)
    {
        DestroyPreviewModel();
        if (lastSelectedNode.character != null) { return; }
        Vector3 offset = character.transform.position - character.GetCharacterTranformToNodePos();
        previewCharacter = Instantiate(character.characterModel);
        previewCharacter.transform.position = lastSelectedNode.GetVector() + offset;
        if (previewMaterial != null)
        {
            MeshRenderer[] renderers = previewCharacter.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.material = previewMaterial;
            }
        }
    }
    public void DestroyPreviewModel()
    {
        if (previewCharacter != null)
        {
            Destroy(previewCharacter);
            previewCharacter = null;
        }
    }
    #endregion
    
    #region Orientation Arrow Gizmos
    public void SetupOrientationArrow(CharacterBase character, GameNode targetNode)
    {
        Orientation orientation = character.orientation;
        orientationArrow.ShowArrows(orientation, targetNode);
    }
    public void HideOrientationArrow()
    {
        orientationArrow.HideAll();
    }
    public bool IsOrientationChanged()
    {
        if (orientationArrow.currentOrientation != lastOrientation)
        {
            lastOrientation = orientationArrow.currentOrientation;
            return true;
        }
        return false;
    }
    public Orientation GetSelectedOrientation()
    {
        return orientationArrow.currentOrientation;
    }
    #endregion

    public void OnLoadNextTurn()
    {
        onLoadNextTurn?.Invoke();
    }

    public List<CharacterBase> GetCharacterOpposites(CharacterBase allyCharacter)
    {
        List<CharacterBase> oppositeUnit = new List<CharacterBase>();
        foreach (TeamDeployment team in battleTeams)
        {
            if (allyCharacter.currentTeam == team) { continue; }
            foreach (CharacterBase opposite in team.teamCharacter)
            {
                oppositeUnit.Add(opposite);
            }
        }
        return oppositeUnit;
    }
    public List<TeamDeployment> GetBattleTeam() => battleTeams;
    public List<CharacterBase> GetBattleUnit() => joinedBattleUnits;

}

