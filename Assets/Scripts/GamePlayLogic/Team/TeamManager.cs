using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public List<TeamDeployment> allTeam = new List<TeamDeployment>();
    public static TeamManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public void GenerateTeam(List<CharacterBase> characters, TeamType teamType)
    {
        if (characters == null || characters.Count == 0) { return; }

        GameObject team = new GameObject($"Teams {allTeam.Count}");
        team.transform.SetParent(transform, false);
        TeamDeployment teamDeployment = team.AddComponent<TeamDeployment>();
        teamDeployment.teamCharacter = characters;
        allTeam.Add(teamDeployment);

        switch (teamType)
        {
            case TeamType.Opposite:
                team.name = team.name + " Opposite";
                EnemyTeamSystem enemyTeamSystem = team.AddComponent<EnemyTeamSystem>();
                enemyTeamSystem.teamDeployment = teamDeployment;
                StartCoroutine(DelayDisable(enemyTeamSystem)); // Debug Used Only
                break;
        }
    }


    //  Debug used only
    private IEnumerator DelayDisable(EnemyTeamSystem enemyTeamSystem)
    {
        yield return null; 
        enemyTeamSystem.SwitchScoutingMode(false);
    }
}
