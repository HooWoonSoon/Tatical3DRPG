using UnityEngine;

public class CharacterPoolsManager : MonoBehaviour
{
    public CharacterBase[] allCharacter;

    public void Start()
    {
        MapManager.instance.onMapSwitchedTrigger += HideAllCharacter;
    }

    private void HideAllCharacter()
    {
        for (int i = 0; i < allCharacter.Length; i++)
        {
            allCharacter[i].gameObject.SetActive(false);
        }
        Debug.Log("All character hidden from the map after deployment.");
    }
}

