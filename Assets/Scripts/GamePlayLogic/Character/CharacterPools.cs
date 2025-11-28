using UnityEngine;

public class CharacterPools : MonoBehaviour
{
    public CharacterBase[] allCharacter;

    public void Start()
    {
        GameEvent.onMapSwitchedTrigger += HideAllCharacter;
    }

    private void HideAllCharacter()
    {
        for (int i = 0; i < allCharacter.Length; i++)
        {
            if (allCharacter[i] != null)
                allCharacter[i].gameObject.SetActive(false);
        }
        Debug.Log("All character hidden from the map after deployment.");
    }
}

