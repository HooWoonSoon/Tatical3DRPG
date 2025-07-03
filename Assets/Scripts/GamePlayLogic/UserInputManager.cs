using System.Collections.Generic;
using UnityEngine;

public class UserInputManager : MonoBehaviour
{
    public TeamDeployment playerTeam;
    private PlayerCharacter selectedCharacter;
    public static UserInputManager instance { get; private set; }
    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    selectedCharacter = playerTeam.GetCharacterData(0);
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    selectedCharacter = playerTeam.GetCharacterData(1);
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    selectedCharacter = playerTeam.GetCharacterData(2);
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    selectedCharacter = playerTeam.GetCharacterData(3);
        //}

        if (selectedCharacter != null)
        {
            CharacterData data = selectedCharacter.data;
            Debug.Log($"Character ID: {data.ID}, Character Name: {data.name}");
        }
    }
}

