using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyTeamSystem : TeamSystem
{
    public TeamDeployment teamDeployment;
    public TeamStateMachine stateMechine;
    public TeamScoutingState teamScoutingState { get; private set; }

    public List<TeamDeployment> detectedTeam = new List<TeamDeployment>();
    public List<CharacterBase> detectedCharacters = new List<CharacterBase>();

    private Vector3 lastPosition;
    private float eslapseTime = 0;

    private void Awake()
    {
        stateMechine = new TeamStateMachine();
        teamScoutingState = new TeamScoutingState(stateMechine, this);
    }
    protected override void Start()
    {
        base.Start();
        stateMechine.Initialize(teamScoutingState);
    }

    private void Update()
    {
        stateMechine.currentEnemyTeamState.Update();
    }

    #region Scouting
    public void TeamSouting()
    {
        foreach (CharacterBase character in teamDeployment.teamCharacter)
        {
            MemberDetectedCharacter(character);
        }

        if (detectedTeam.Count == 0 || detectedCharacters.Count == 0) { return; }
        
        GetInfluenceUnits(detectedTeam, out List<CharacterBase> joinedBattleUnit);
        List<PathRoute> pathRoutes = GetGridBattlePath(joinedBattleUnit);
        for (int i = 0; i < joinedBattleUnit.Count; i++)
        {
            CTTimeline.instance.InsertCharacter(joinedBattleUnit[i]);
            joinedBattleUnit[i].pathRoute = pathRoutes[i];
            joinedBattleUnit[i].EnterBattle();
        }
        List<TeamDeployment> allTeam = new List<TeamDeployment>();
        allTeam.AddRange(detectedTeam);
        allTeam.Add(teamDeployment);

        BattleManager.instance.SetJoinedTeam(allTeam);
        BattleManager.instance.SetJoinedBattleUnit(joinedBattleUnit);
        CTTimeline.instance.SetupTimeline();
    }

    private void MemberDetectedCharacter(CharacterBase character)
    {
        UnitDetectable[] unitDetectable = character.detectable.OverlapMahhatassRange(5);

        foreach (UnitDetectable hit in unitDetectable)
        {
            CharacterBase detectedCharacter = hit.GetComponent<CharacterBase>();
            detectedCharacters.Add(detectedCharacter);
            TeamDeployment dectectTeam = detectedCharacter.currentTeam;
            if (dectectTeam != null && !IsSameTeamMember(dectectTeam))
            {
                Debug.Log("true, Inside Mahhatass Range");
                if (!detectedTeam.Contains(dectectTeam))
                {
                    detectedTeam.Add(dectectTeam);
                }
            }
        }
    }

    private void GetInfluenceUnits(List<TeamDeployment> allDetectedTeam, out List<CharacterBase> joinedBattleUnit)
    {
        joinedBattleUnit = new List<CharacterBase>();

        foreach (TeamDeployment team in allDetectedTeam)
        {
            foreach (CharacterBase character in team.teamCharacter)
            {
                if (!joinedBattleUnit.Contains(character))
                    joinedBattleUnit.Add(character);
            }
        }

        foreach (var member in teamDeployment.teamCharacter)
        {
            if (!joinedBattleUnit.Contains(member))
                joinedBattleUnit.Add(member);
        }
    }

    private bool IsSameTeamMember(TeamDeployment team)
    {
        if (teamDeployment == team) { return true; }
        return false;
    }
    #endregion
}

