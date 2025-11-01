using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class MapTeamManager : MonoBehaviour
{
    public List<TeamDeployment> allTeam = new List<TeamDeployment>();
    public static MapTeamManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public void RemoveTeam(TeamType teamType)
    {
        List<TeamDeployment> toRemove = new List<TeamDeployment>();

        foreach (TeamDeployment team in allTeam)
        {
            if (team.teamType == teamType)
            {
                toRemove.Add(team);
            }
        }
        RemoveTeams(toRemove);
    }
    public void RemoveTeams(List<TeamDeployment> teams)
    {
        foreach (TeamDeployment team in teams)
        {
            RemoveTeam(team);
        }
    }
    public void RemoveTeam(TeamDeployment team)
    {
        if (allTeam.Contains(team))
        {
            allTeam.Remove(team);
            if (team != null && team.gameObject != null)
            {
                Destroy(team.gameObject);
            }
        }
    }

    public void GenerateTeam(List<CharacterBase> characters, TeamType teamType, bool isDeploy = false)
    {
        if (characters == null || characters.Count == 0) { return; }

        GameObject team = new GameObject($"Teams System");
        team.transform.SetParent(transform, false);
        TeamDeployment teamDeployment = team.AddComponent<TeamDeployment>();
        teamDeployment.teamCharacter = characters;
        allTeam.Add(teamDeployment);

        switch (teamType)
        {
            case TeamType.Opposite:
                teamDeployment.teamType = TeamType.Opposite;
                team.name = team.name + " Opposite" + allTeam.Count.ToString();
                EnemyTeamSystem enemyTeamSystem = team.AddComponent<EnemyTeamSystem>();
                if (isDeploy)
                {
                    enemyTeamSystem.Initialize<TeamIdleState>(teamDeployment);
                    //Debug.Log("Generate Team Deploy");
                }
                else
                {
                    enemyTeamSystem.Initialize<TeamScoutingState>(teamDeployment);
                    //Debug.Log("Generate Team Scouting");
                }
                break;
        }
    }
}
