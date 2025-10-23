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

    public void SetJoinedBattleUnit(HashSet<CharacterBase> joinedBattleUnits)
    {
        this.joinedBattleUnits = joinedBattleUnits.ToList();
    }

    public void SetJoinedBattleUnit(List<CharacterBase> joinedBattleUnits)
    {
        this.joinedBattleUnits = joinedBattleUnits;
    }

    #region Battle Unit Refine Path
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

    public void SetGridCursorAt(GameNode target)
    {
        gridCursor.HandleGridCursor(target);
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
        DestroyPreviewModel();
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

    public void DestroyPreviewModel()
    {
        if (previewCharacter != null)
        {
            Destroy(previewCharacter);
            previewCharacter = null;
        }
    }

    public void OnLoadNextTurn()
    {
        onLoadNextTurn?.Invoke();
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

    #region Visual Handle
    public void ActivateMoveCursorAndHide(bool active, bool hide)
    {
        gridCursor.ActivateMoveCursor(active, hide);
    }

    public void SetupOrientationArrow(CharacterBase character, GameNode targetNode)
    {
        Orientation orientation = character.orientation;
        orientationArrow.ShowArrows(orientation, targetNode);
    }

    public void HideOrientationArrow()
    {
        orientationArrow.HideAll();
    }
    #endregion

    public List<TeamDeployment> GetBattleTeam() => battleTeams;
    public List<CharacterBase> GetBattleUnit() => joinedBattleUnits;

}

