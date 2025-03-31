using UnityEngine;
using System.Collections.Generic;

public class TeamManagerTesting : MonoBehaviour
{
    private Team[] teams;
    [SerializeField] private List<UnitCharacter> unitCharacters = new List<UnitCharacter>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            InitializeTeam();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ResetTeam();
        }
    }

    private void InitializeTeam()
    {
        teams = new Team[4];
        for (int i = 0; i < teams.Length; i++)
        {
            teams[i] = new Team();
        }

        if (unitCharacters.Count > 0)
        {
            for (int i = 0; i < unitCharacters.Count; i++)
            {
                teams[0].AddMember(unitCharacters[i]);
            }
            Debug.Log($"A: {teams[0].members.Count}");
            Debug.Log("IsReadyTeam");
        }
    }

    private void ResetTeam()
    {
        foreach (Team team in teams)
        {
            team.ResetMember();
            Debug.Log($"R: {team.members.Count}");
        }
        Debug.Log("IsResetTeam");
    }
}