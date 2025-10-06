using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyTeamSystem : TeamSystem
{
    public TeamDeployment teamDeployment;
    public TeamStateMachine stateMechine;
    public TeamScoutingState teamScoutingState { get; private set; }

    public HashSet<TeamDeployment> detectedTeam = new HashSet<TeamDeployment>();
    public List<CharacterBase> detectedCharacters = new List<CharacterBase>();
    public HashSet<CharacterBase> lastUnit = new HashSet<CharacterBase>();

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
            DetectedEntireTeamCharacter(character);
        }

        if (detectedCharacters.Count == 0 || detectedTeam.Count == 0) { return; }
        HashSet<CharacterBase> allUnit = GetDetectableAndSelfTeamUnit();
        if (lastUnit.SetEquals(allUnit)) { return; }
        //Debug.Log("Last Unit different");
        lastUnit = new HashSet<CharacterBase>(allUnit);
        BattleManager.instance.SetJoinedBattleUnit(allUnit);
        BattleManager.instance.PreapreBattleContent();
    }

    private HashSet<CharacterBase> GetDetectableAndSelfTeamUnit()
    {
        HashSet<CharacterBase> result = new HashSet<CharacterBase>();
        foreach (CharacterBase character in teamDeployment.teamCharacter)
        {
            if (!result.Contains(character))
            {
                result.Add(character);
            }
        }
        foreach (CharacterBase character in detectedCharacters)
        {
            if (!result.Contains(character))
            {
                result.Add(character);
            }
        }
        return result;
    }

    private void DetectedEntireTeamCharacter(CharacterBase character)
    {
        UnitDetectable[] unitDetectable = character.detectable.OverlapMahhatassRange(5);

        foreach (UnitDetectable hit in unitDetectable)
        {
            CharacterBase detectedCharacter = hit.GetComponent<CharacterBase>();

            if (detectedCharacter == null) { continue; }
            if (IsSameTeamMember(detectedCharacter)) { continue; }

            TeamDeployment dectectTeam = detectedCharacter.currentTeam;
            if (IsSameTeam(dectectTeam)) { continue;}

            List<CharacterBase> dectectTeamCharacter = dectectTeam.teamCharacter;
            if (!detectedTeam.Contains(dectectTeam))
            {
                detectedTeam.Add(dectectTeam);
            }
            foreach (CharacterBase teamCharacter in dectectTeamCharacter)
            {
                if (!detectedCharacters.Contains(teamCharacter))
                {
                    detectedCharacters.Add(teamCharacter);
                }
            }
        }
    }

    private bool IsSameTeam(TeamDeployment team)
    {
        if (teamDeployment == team) { return true; }
        else if (team == null) { return false; }
        return false;
    }

    private bool IsSameTeamMember(CharacterBase character)
    {
        if (character.currentTeam == teamDeployment) { return true; }
        return false;
    }
    #endregion
}

