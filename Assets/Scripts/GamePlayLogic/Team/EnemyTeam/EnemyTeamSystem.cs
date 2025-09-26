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
        bool hasNewDetected = false;
        foreach (CharacterBase character in teamDeployment.teamCharacter)
        {
            hasNewDetected = IsDetectedCharacter(character); 

            if (!hasNewDetected) { return; }
            if (detectedTeam.Count == 0 || detectedCharacters.Count == 0) { return; }

            GetInfluenceUnits(detectedTeam, out List<CharacterBase> joinedBattleUnit);

            BattleManager.instance.SetJoinedBattleUnit(joinedBattleUnit);
            BattleManager.instance.PreapreBattleContent();
        }
    }

    private bool IsDetectedCharacter(CharacterBase character)
    {
        bool newDetected = false;
        UnitDetectable[] unitDetectable = character.detectable.OverlapMahhatassRange(5);

        foreach (UnitDetectable hit in unitDetectable)
        {
            CharacterBase detectedCharacter = hit.GetComponent<CharacterBase>();
            if (detectedCharacter == null) { continue; }

            if (!detectedCharacters.Contains(detectedCharacter))
            {
                detectedCharacters.Add(detectedCharacter);
                TeamDeployment dectectTeam = detectedCharacter.currentTeam;
                if (!IsSameTeam(dectectTeam))
                {
                    detectedTeam.Add(dectectTeam);
                }
                newDetected = true;
            }
        }
        return newDetected;
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

    private bool IsSameTeam(TeamDeployment team)
    {
        if (teamDeployment == team) { return true; }
        else if (team == null) { return false; }
        return false;
    }
    #endregion
}

