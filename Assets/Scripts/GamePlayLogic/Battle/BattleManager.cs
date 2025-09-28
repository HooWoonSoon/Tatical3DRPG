using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleManager : Entity
{
    public static BattleManager instance { get; private set; }
    public BattleUI battleUI;

    public List<TeamDeployment> battleTeams = new List<TeamDeployment>();
    public List<CharacterBase> joinedBattleUnits = new List<CharacterBase>();
    public bool isBattleStarted = false;

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
        if (isBattleStarted) { return; }
        Debug.Log("PreapareBattleContent");
        CTTimeline.instance.SetJoinedBattleUnit(joinedBattleUnits);
        CTTimeline.instance.SetupTimeline();
        FindJoinedTeam();
        EnterBattleUnitRefinePath();
        
        battleUI.OnBattleUIFinish += StartBattle;
        battleUI.PlayBattleUI();
    }

    public void StartBattle()
    {
        Debug.Log("StartBattle");
        isBattleStarted = true;
    }

    public List<TeamDeployment> GetBattleTeam() => battleTeams;
    public List<CharacterBase> GetBattleUnit() => joinedBattleUnits;

}

