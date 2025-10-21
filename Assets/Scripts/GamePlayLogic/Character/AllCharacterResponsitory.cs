using UnityEngine;

public class AllCharacterResponsitory : MonoBehaviour
{
    public CharacterBase[] allCharacter;

    public void Start()
    {
        MapDeploymentManager.instance.onDeploymentTrigger += HideAllCharacter;
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

