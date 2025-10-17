using UnityEngine;
public class AvailableCharacterManager : MonoBehaviour
{
    public CharacterBase[] allCharacter;
    public static AvailableCharacterManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }
}