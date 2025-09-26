using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleManager : Entity
{
    public static BattleManager instance { get; private set; }

    public List<TeamDeployment> battleTeams = new List<TeamDeployment>();
    public List<CharacterBase> joinedBattleUnits = new List<CharacterBase>();
    public bool isBattleStarted = false;

    public CTTurnUIGenerator ctTurnUIGenerator;

    [Header("Battle Cursor")]
    [SerializeField] private GameObject cursor;
    [SerializeField] private float heightOffset = 2.5f;
    private bool activateCursor = false;
    private float keyPressTimer;
    private float intervalPressTimer;

    public event Action onInfoTargetChanged;

    private void Awake()
    {
        instance = this;
    }
    protected override void Start()
    {
        base.Start();
        CTTimeline.instance.confirmCTTimeline += HandleBattle;
        cursor.SetActive(false);
    }
    private void Update()
    {
        if (activateCursor)
        {
            HandleInput(KeyCode.W, Vector3Int.forward, 0.25f, 0.025f);
            HandleInput(KeyCode.S, Vector3Int.back, 0.25f, 0.025f);
            HandleInput(KeyCode.A, Vector3Int.left, 0.25f, 0.025f);
            HandleInput(KeyCode.D, Vector3Int.right, 0.25f, 0.025f);
        }
    }

    private void HandleInput(KeyCode keyCode, Vector3Int direction, float initialTimer, float interval)
    {
        if (Input.GetKeyDown(keyCode))
        {
            keyPressTimer = 0;
            TryMove(direction);
        }
        if (Input.GetKey(keyCode))
        {
            keyPressTimer += Time.deltaTime;
            if (keyPressTimer > initialTimer)
            {
                intervalPressTimer += Time.deltaTime;
                if (intervalPressTimer >= interval)
                {
                    TryMove(direction);
                    intervalPressTimer = 0;
                }
            }
        }
    }
    private void TryMove(Vector3Int direction)
    {
        Vector3Int nodePos = Utils.RoundXZFloorYInt(cursor.transform.position);
        GameNode gameNode = world.GetHeightNodeWithCube(nodePos.x + direction.x, nodePos.z + direction.z);
        if (gameNode != null)
        {
            cursor.transform.position = gameNode.GetGameNodeVector() + new Vector3(0, heightOffset);
            CharacterBase character = gameNode.GetUnitGridCharacter();
            if (character != null)
            {
                ctTurnUIGenerator.TargetCursorCharacterUI(character);
            }
        }
    }

    public void HandleBattle()
    {
        for (int i = 0; i < joinedBattleUnits.Count; i++)
        {
            joinedBattleUnits[i].ReadyBattle();
        }
        CharacterBase character = CTTimeline.instance.GetCurrentCharacter();
        ctTurnUIGenerator.TargetCursorCharacterUI(character);
        cursor.SetActive(true);
        cursor.transform.position = Utils.RoundXZFloorYInt(character.transform.position);
        activateCursor = true;
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
            joinedBattleUnits[i].pathRoute = pathRoutes[i];
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
        CTTimeline.instance.SetJoinedBattleUnit(joinedBattleUnits);
        CTTimeline.instance.SetupTimeline();
        FindJoinedTeam();
        EnterBattleUnitRefinePath();
    }

    public List<TeamDeployment> GetBattleTeam() => battleTeams;

}

