using UnityEngine;

public class TeamFollowSystem : MonoBehaviour
{
    private UnitCharacter leaderCharacter;
    [SerializeField] private int historyLimited = 500;

    public void SetLeader(UnitCharacter unit)
    {
        leaderCharacter = unit;
    }

    [SerializeField] private TeamUIController teamUIController;
    public float baseSpacing = 2f;


    private void GetMemberTeamIndex(out int index)
    {
        index = -1;
        for (int i = 0; i < teamUIController.teamUIClasses.Length; i++)
        {
            //index = teamUIController.teamUIClasses[i].index;
        }
    }
}
