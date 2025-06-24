using UnityEngine;
using UnityEngine.UI;

public class TeamLinkButton : MonoBehaviour
{
    public TeamSystem teamSystem;
    public UILinkTooltip uILinkTooltip;
    private TeamLinkUIClass currentTeamUIClass;

    public Button linkOrUnlinkButton;
    public Button detailButton;

    public void Initialize(TeamLinkUIClass currentTeamUIClass)
    {
        this.currentTeamUIClass = currentTeamUIClass;

        linkOrUnlinkButton.onClick.RemoveAllListeners();
        detailButton.onClick.RemoveAllListeners();

        linkOrUnlinkButton.onClick.AddListener(() => OnClickLinkUnlinkButton());
        detailButton.onClick.AddListener(() => OnClickDetailButton());
    }

    public void OnClickLinkUnlinkButton()
    {
        Debug.Log($"Unlink button clicked with index: {currentTeamUIClass.index}");
        if (currentTeamUIClass.character.isLink == true)
        {
            currentTeamUIClass.UnlinkCharacter();
            teamSystem.RemoveUnlinkCharacterFromTeam(currentTeamUIClass.character);
            teamSystem.AddCharacterToUnlinkList(currentTeamUIClass.character);
            TeamFollowPathFinding.instance.ResetTeamPathRoute();
        }
        else
        {
            currentTeamUIClass.LinkCharacter();
            teamSystem.InsertTeamFollower(currentTeamUIClass.character);
            TeamFollowPathFinding.instance.ResetTeamPathRoute();
        }
    }

    public void OnClickDetailButton()
    {
        Debug.Log("Detail button clicked with index: " + currentTeamUIClass.index);
    }
}
