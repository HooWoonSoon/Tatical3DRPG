using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : Entity
{
    public List<TeamDeployment> battleTeams = new List<TeamDeployment>();
    public List<CharacterBase> joinedBattleUnits = new List<CharacterBase>();
    public bool isBattleStarted = false;
    public BattleCursor battleCursor;
    private GameNode lastSelectedNode;

    [Header("Preview")]
    public Material previewMaterial;
    private GameObject previewCharacter;


    public static BattleManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }
    protected override void Start()
    {
        base.Start();
    }

    public void SetJoinedBattleUnit(HashSet<CharacterBase> joinedBattleUnits)
    {
        this.joinedBattleUnits = joinedBattleUnits.ToList();
    }

    public void SetJoinedBattleUnit(List<CharacterBase> joinedBattleUnits)
    {
        this.joinedBattleUnits = joinedBattleUnits;
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

    private void EnterBattleUnitRefinePath()
    {
        List<PathRoute> pathRoutes = GetBattleUnitRefinePath();

        for (int i = 0; i < joinedBattleUnits.Count; i++)
        {
            joinedBattleUnits[i].SetPathRoute(pathRoutes[i]);
            joinedBattleUnits[i].ReadyBattle();
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
            Vector3Int unitPosition = character.GetCharacterNodePosition();
            while (iteration <= 16 && !found)
            {
                List<Vector3Int> optionPos = character.GetUnlimitedMovablePos(iteration);
                List<Vector3Int> sortPos = Utils.SortTargetRangeByDistance(unitPosition, optionPos);
                for (int i = 0; i < sortPos.Count; i++)
                {
                    if (!occupiedPos.Contains(sortPos[i]))
                    {
                        List<Vector3> pathVectorList = (pathFinding.GetPathRoute(unitPosition, sortPos[i], 1, 1).pathRouteList);
                        if (pathVectorList.Count != 0)
                        {
                            pathRoutes.Add(new PathRoute
                            {
                                character = character,
                                targetPosition = sortPos[i],
                                pathRouteList = pathVectorList,
                                pathIndex = 0
                            });
                            occupiedPos.Add(sortPos[i]);
                            found = true;
                            break;
                        }
                    }
                }
                iteration++;
            }

            if (!found)
            {
                Debug.LogError($"{character.name} Not Found Path");
                return null;
            }
        }
        return pathRoutes;
    }

    public void PreapreBattleContent()
    {
        if (isBattleStarted) { return; }
        Debug.Log("PreapareBattleContent");
        FindJoinedTeam();
        EnterBattleUnitRefinePath();
        BattleUIManager.instance.PrepareBattleUI();
        CTTimeline.instance.SetJoinedBattleUnit(joinedBattleUnits);
        CTTimeline.instance.SetupTimeline();
        
        BattleUIManager.instance.OnBattleUIFinish += StartBattle;
    }

    public void StartBattle()
    {
        Debug.Log("StartBattle");
        isBattleStarted = true;
    }

    public void SetBattleCursorAt(GameNode target)
    {
        battleCursor.HandleBattleCursor(target);
    }

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
        DestoryPreviewModel();
        if (lastSelectedNode.character != null) { return; }
        Vector3 offset = character.transform.position - character.GetCharacterNodePosition();
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

    public void DestoryPreviewModel()
    {
        if (previewCharacter != null)
        {
            Destroy(previewCharacter);
            previewCharacter = null;
        }
    }

    public GameNode GetSelectedGameNode()
    {
        return battleCursor.currentGameNode;
    }
    public bool IsSelectedNodeChange()
    {
        if (battleCursor.currentGameNode != lastSelectedNode)
        {
            lastSelectedNode = battleCursor.currentGameNode;
            return true;
        }
        return false;
    }
    public void ActivateMoveCursor(bool active)
    {
        battleCursor.ActivateMoveCursor(active);
    }
    public List<TeamDeployment> GetBattleTeam() => battleTeams;
    public List<CharacterBase> GetBattleUnit() => joinedBattleUnits;

}

