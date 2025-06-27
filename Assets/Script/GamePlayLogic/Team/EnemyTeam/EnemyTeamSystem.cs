using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTeamSystem : Entity
{
    public TeamDeployment teamDeployment;
    public TeamStateMachine stateMechine;
    public TeamScoutingState teamScoutingState { get; private set; }

    public List<CharacterBase> allDetectedCharacter = new List<CharacterBase>();

    private Vector3 lastPosition;
    private float eslapseTime = 0;

    public event Action<List<CharacterBase>> OnBattleTriggered;

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

        if (allDetectedCharacter.Count > 0)
        {
            GetInfluenceUnits(allDetectedCharacter, out List<CharacterBase> joinedBattleUnit);
            OnBattleTriggered?.Invoke(joinedBattleUnit);
        }
    }

    private void MemberDetectedCharacter(CharacterBase character)
    {
        UnitDetectable[] unitDetectable = character.detectable.OverlapMahhatassRange(5);

        foreach (UnitDetectable hit in unitDetectable)
        {
            CharacterBase hitCharacter = hit.GetComponent<CharacterBase>();
            if (hitCharacter != null && !IsSameTeamMember(hitCharacter))
            {
                Debug.Log("true, Inside Mahhatass Range");
                if (!allDetectedCharacter.Contains(hitCharacter))
                {
                    allDetectedCharacter.Add(hitCharacter);
                }
            }
        }
    }

    private void GetInfluenceUnits(List<CharacterBase> detectedCharacters, out List<CharacterBase> joinedBattleUnit)
    {
        joinedBattleUnit = new List<CharacterBase>();

        foreach (var detected in detectedCharacters)
        {
            List<CharacterBase> team = detected.currentTeam.teamCharacter;
            foreach (var member in team)
            {
                if (!joinedBattleUnit.Contains(member))
                    joinedBattleUnit.Add(member);
            }
        }

        foreach (var member in teamDeployment.teamCharacter)
        {
            if (!joinedBattleUnit.Contains(member))
                joinedBattleUnit.Add(member);
        }
    }

    private bool IsSameTeamMember(CharacterBase character)
    {
        return teamDeployment.teamCharacter.Contains(character);
    }
    #endregion
}

