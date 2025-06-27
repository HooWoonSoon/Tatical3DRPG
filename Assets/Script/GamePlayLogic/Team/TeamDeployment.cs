using System.Collections.Generic;
using UnityEngine;

public class TeamDeployment : MonoBehaviour
{
    public List<CharacterBase> teamCharacter = new List<CharacterBase>();

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
    
    public CharacterBase GetCharacterData(int index)
    {
        for (int i = 0; i < teamCharacter.Count; i++)
        {
            if (teamCharacter[i].index == index)
            {
                return teamCharacter[i];
            }
        }
        Debug.Log("Character with index " + index + " not found in team.");
        return null;
    }
}