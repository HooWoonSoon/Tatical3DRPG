using System.Collections.Generic;
using UnityEngine;

public class TeamDeployment : MonoBehaviour
{
    public List<Character> teamCharacter = new List<Character>();

    private void Start()
    {
        // Initialize team characters
        for (int i = 0; i < teamCharacter.Count; i++)
        {
            if (teamCharacter[i] != null)
            {
                teamCharacter[i].currentTeam = this;
                teamCharacter[i].index = i;
            }
        }
    }
}